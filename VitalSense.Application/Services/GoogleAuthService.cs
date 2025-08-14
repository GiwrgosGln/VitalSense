using Microsoft.Extensions.Configuration;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace VitalSense.Application.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IConfiguration configuration,
        IUserService userService,
        ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _userService = userService;
        _logger = logger;
    }

    public Task<GoogleAuthUrlResponse> GetAuthorizationUrlAsync(Guid userId)
    {
        var clientId = _configuration["Google:ClientId"];
        var redirectUri = _configuration["Google:RedirectUri"];
        var scope = "https://www.googleapis.com/auth/calendar";

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogError("Google ClientId is not configured");
            throw new InvalidOperationException("Google ClientId is not configured. Please check your application settings.");
        }

        if (string.IsNullOrEmpty(redirectUri))
        {
            _logger.LogError("Google RedirectUri is not configured");
            throw new InvalidOperationException("Google RedirectUri is not configured. Please check your application settings.");
        }

        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&response_type=code" +
            $"&state={userId}" +
            "&access_type=offline" +
            "&prompt=consent";

        return Task.FromResult(new GoogleAuthUrlResponse { AuthUrl = authUrl });
    }

    public async Task<GoogleCalendarConnectionResponse> HandleOAuthCallbackAsync(string code, Guid userId)
    {
        try
        {
            var tokenResponse = await ExchangeCodeForTokensAsync(code);
            if (tokenResponse == null)
            {
                return new GoogleCalendarConnectionResponse 
                { 
                    Success = false, 
                    Message = "Failed to exchange code for tokens" 
                };
            }

            var expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn ?? 3600);
            var success = await _userService.UpdateGoogleTokensAsync(
                userId, 
                tokenResponse.AccessToken!, 
                tokenResponse.RefreshToken ?? "", 
                expiry);

            return new GoogleCalendarConnectionResponse
            {
                Success = success,
                Message = success ? "Google Calendar connected successfully" : "Failed to save tokens"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for user {UserId}", userId);
            return new GoogleCalendarConnectionResponse 
            { 
                Success = false, 
                Message = "An error occurred during connection" 
            };
        }
    }

    public async Task<bool> DisconnectGoogleCalendarAsync(Guid userId)
    {
        return await _userService.ClearGoogleTokensAsync(userId);
    }

    public async Task<GoogleCalendarStatusResponse> GetConnectionStatusAsync(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return new GoogleCalendarStatusResponse
        {
            IsConnected = user?.IsGoogleCalendarConnected ?? false,
            ConnectedDate = user?.GoogleTokenExpiry?.AddSeconds(-3600)
        };
    }

    public async Task<string?> GetValidAccessTokenAsync(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null || !user.IsGoogleCalendarConnected)
        {
            return null;
        }

        _logger.LogInformation("Checking access token validity for user {UserId}", userId);
        
        if (user.GoogleTokenExpiry > DateTime.UtcNow.AddMinutes(5))
        {
            _logger.LogInformation("Access token is still valid for user {UserId}", userId);
            return user.GoogleAccessToken;
        }

        _logger.LogInformation("Access token expired for user {UserId}, attempting refresh", userId);

        if (!string.IsNullOrEmpty(user.GoogleRefreshToken))
        {
            var newToken = await RefreshTokenAsync(user.GoogleRefreshToken);
            if (newToken != null)
            {
                _logger.LogInformation("Successfully refreshed access token for user {UserId}", userId);
                
                var expiry = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn ?? 3600);
                await _userService.UpdateGoogleTokensAsync(
                    userId, 
                    newToken.AccessToken!, 
                    newToken.RefreshToken ?? user.GoogleRefreshToken, 
                    expiry);
                
                return newToken.AccessToken;
            }
            else
            {
                _logger.LogWarning("Failed to refresh access token for user {UserId}", userId);
            }
        }
        else
        {
            _logger.LogWarning("No refresh token available for user {UserId}", userId);
        }

        return null;
    }

    public async Task<bool> RefreshUserTokenAsync(Guid userId)
    {
        var token = await GetValidAccessTokenAsync(userId);
        return !string.IsNullOrEmpty(token);
    }

    private async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];
            var redirectUri = _configuration["Google:RedirectUri"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            {
                _logger.LogError("Google configuration is incomplete. ClientId, ClientSecret, and RedirectUri are required.");
                return null;
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var parameters = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"redirect_uri", redirectUri},
                {"grant_type", "authorization_code"}
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            
            if (!response.IsSuccessStatusCode) 
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to exchange code for tokens. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout exchanging code for tokens");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            return null;
        }
    }

    private async Task<GoogleTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Google ClientId and ClientSecret are required for token refresh.");
                return null;
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var parameters = new Dictionary<string, string>
            {
                {"refresh_token", refreshToken},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"grant_type", "refresh_token"}
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            
            if (!response.IsSuccessStatusCode) 
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to refresh Google token. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTokenResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout refreshing Google token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Google token");
            return null;
        }
    }
}
