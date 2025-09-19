using VitalSense.Application.DTOs;

namespace VitalSense.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleAuthUrlResponse> GetAuthorizationUrlAsync(Guid userId);
    Task<GoogleCalendarConnectionResponse> HandleOAuthCallbackAsync(string code, Guid userId);
    Task<bool> DisconnectGoogleCalendarAsync(Guid userId);
    Task<GoogleCalendarStatusResponse> GetConnectionStatusAsync(Guid userId);
    Task<string?> GetValidAccessTokenAsync(Guid userId);
    Task<bool> RefreshUserTokenAsync(Guid userId);
    Task<GoogleTokenResponse?> RefreshTokenDirectAsync(string refreshToken);
}
