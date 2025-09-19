using Microsoft.Extensions.Configuration;
using VitalSense.Domain.Entities;
using VitalSense.Application.Interfaces;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace VitalSense.Application.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(
        IUserService userService,
        IGoogleAuthService googleAuthService,
        ILogger<GoogleCalendarService> logger)
    {
        _userService = userService;
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    public async Task<bool> SyncAppointmentToGoogleAsync(Appointment appointment)
    {
        var user = await _userService.GetByIdAsync(appointment.DieticianId);
        return await SyncAppointmentToGoogleAsync(appointment, user);
    }

    public async Task<bool> SyncAppointmentToGoogleAsync(Appointment appointment, User? user)
    {
        if (user?.IsGoogleCalendarConnected != true) return false;

        try
        {
            var accessToken = await GetValidAccessTokenAsync(user);
            if (accessToken == null) return false;

            // Check if appointment is already synced to Google Calendar
            if (!string.IsNullOrEmpty(appointment.GoogleEventId))
            {
                // First, verify the event still exists in Google Calendar
                var eventExists = await GoogleCalendarEventExistsAsync(appointment.GoogleEventId, user);
                
                if (eventExists)
                {
                    // Update existing event
                    _logger.LogInformation("Appointment {AppointmentId} is already synced. Updating existing Google Calendar event {GoogleEventId}", 
                        appointment.Id, appointment.GoogleEventId);
                    return await UpdateAppointmentInGoogleAsync(appointment, user);
                }
                else
                {
                    // Event was deleted from Google Calendar, clear the ID and create a new one
                    _logger.LogWarning("Google Calendar event {GoogleEventId} for appointment {AppointmentId} no longer exists. Creating new event.", 
                        appointment.GoogleEventId, appointment.Id);
                    
                    appointment.GoogleEventId = null; // Clear the stale reference
                    
                    // Create new event and update database
                    var (success, eventId) = await CreateAppointmentInGoogleAsync(appointment, user);
                    
                    if (success && !string.IsNullOrEmpty(eventId))
                    {
                        appointment.GoogleEventId = eventId;
                        _logger.LogInformation("Created new Google Calendar event {GoogleEventId} for appointment {AppointmentId}", 
                            eventId, appointment.Id);
                    }
                    
                    return success;
                }
            }
            else
            {
                // Create new event
                _logger.LogInformation("Appointment {AppointmentId} not yet synced. Creating new Google Calendar event", appointment.Id);
                var (success, eventId) = await CreateAppointmentInGoogleAsync(appointment, user);
                
                if (success && !string.IsNullOrEmpty(eventId))
                {
                    // Update the appointment with the Google Event ID
                    appointment.GoogleEventId = eventId;
                    _logger.LogInformation("Assigned Google Event ID {GoogleEventId} to appointment {AppointmentId}", 
                        eventId, appointment.Id);
                }
                
                return success;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing appointment {AppointmentId} to Google Calendar", appointment.Id);
            return false;
        }
    }

    public async Task<bool> UnsyncAppointmentFromGoogleAsync(Appointment appointment)
    {
        var user = await _userService.GetByIdAsync(appointment.DieticianId);
        return await UnsyncAppointmentFromGoogleAsync(appointment, user);
    }

    public async Task<bool> UnsyncAppointmentFromGoogleAsync(Appointment appointment, User? user)
    {
        if (user?.IsGoogleCalendarConnected != true) 
        {
            _logger.LogWarning("User {UserId} is not connected to Google Calendar", appointment.DieticianId);
            return false;
        }

        // Check if appointment is actually synced
        if (string.IsNullOrEmpty(appointment.GoogleEventId))
        {
            _logger.LogInformation("Appointment {AppointmentId} is not synced to Google Calendar", appointment.Id);
            return true; // Consider this a success since the desired state is achieved
        }

        try
        {
            // First, check if the event still exists in Google Calendar
            var eventExists = await GoogleCalendarEventExistsAsync(appointment.GoogleEventId, user);
            
            if (!eventExists)
            {
                // Event was already deleted externally, just clear the database reference
                _logger.LogInformation("Google Calendar event {GoogleEventId} for appointment {AppointmentId} was already deleted externally. Clearing database reference.", 
                    appointment.GoogleEventId, appointment.Id);
                
                appointment.GoogleEventId = null;
                return true;
            }

            // Event exists, proceed with deletion
            var success = await DeleteAppointmentFromGoogleAsync(appointment, user);
            
            if (success)
            {
                // Clear the GoogleEventId from the appointment
                appointment.GoogleEventId = null;
                _logger.LogInformation("Successfully unsynced appointment {AppointmentId} from Google Calendar", appointment.Id);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsyncing appointment {AppointmentId} from Google Calendar", appointment.Id);
            return false;
        }
    }

    public async Task<bool> GoogleCalendarEventExistsAsync(string googleEventId, User user)
    {
        if (string.IsNullOrEmpty(googleEventId) || user?.IsGoogleCalendarConnected != true)
        {
            return false;
        }

        try
        {
            var accessToken = await GetValidAccessTokenAsync(user);
            if (accessToken == null) return false;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(
                $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{googleEventId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Google Calendar event {GoogleEventId} exists", googleEventId);
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Google Calendar event {GoogleEventId} not found (may have been deleted)", googleEventId);
                return false;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to check Google Calendar event {GoogleEventId}. Status: {StatusCode}, Error: {Error}", 
                    googleEventId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout checking Google Calendar event {GoogleEventId}", googleEventId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Google Calendar event {GoogleEventId}", googleEventId);
            return false;
        }
    }

    public async Task<bool> ValidateAndCleanupStaleReferenceAsync(Appointment appointment, User user)
    {
        if (string.IsNullOrEmpty(appointment.GoogleEventId) || user?.IsGoogleCalendarConnected != true)
        {
            return false; // Nothing to validate
        }

        var eventExists = await GoogleCalendarEventExistsAsync(appointment.GoogleEventId, user);
        
        if (!eventExists)
        {
            _logger.LogInformation("Cleaning up stale Google Calendar reference for appointment {AppointmentId}. Event {GoogleEventId} no longer exists.", 
                appointment.Id, appointment.GoogleEventId);
            
            appointment.GoogleEventId = null;
            return true; // Indicates cleanup was performed
        }

        return false; // No cleanup needed
    }

    public async Task<bool> UpdateAppointmentInGoogleAsync(Appointment appointment)
    {
        var user = await _userService.GetByIdAsync(appointment.DieticianId);
        if (user == null) return false;
        return await UpdateAppointmentInGoogleAsync(appointment, user);
    }

    public async Task<bool> UpdateAppointmentInGoogleAsync(Appointment appointment, User user)
    {
        if (string.IsNullOrEmpty(user.GoogleAccessToken) || 
            string.IsNullOrEmpty(user.GoogleRefreshToken) || 
            user.GoogleTokenExpiry <= DateTime.UtcNow)
        {
            _logger.LogWarning("User {UserId} doesn't have valid Google credentials", appointment.DieticianId);
            return false;
        }

        try
        {
            var accessToken = user.GoogleAccessToken;
            
            // Check if we need to refresh the token
            if (user.GoogleTokenExpiry <= DateTime.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("Access token expired or close to expiry, refresh needed. Current expiry: {Expiry}", user.GoogleTokenExpiry);
                
                var newToken = await _googleAuthService.RefreshTokenDirectAsync(user.GoogleRefreshToken);
                if (newToken != null && !string.IsNullOrEmpty(newToken.AccessToken))
                {
                    accessToken = newToken.AccessToken;
                    _logger.LogInformation("Successfully refreshed token directly");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh token");
                    return false;
                }
            }

            var googleEvent = CreateGoogleEvent(appointment);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var json = JsonSerializer.Serialize(googleEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (string.IsNullOrEmpty(appointment.GoogleEventId))
            {
                _logger.LogWarning("No GoogleEventId found for appointment {AppointmentId}. This may result in duplicate calendar events.", appointment.Id);
                return false;
            }

            // Use PUT to update existing event
            var response = await httpClient.PutAsync(
                $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{appointment.GoogleEventId}", 
                content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated appointment {AppointmentId} in Google Calendar", appointment.Id);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update appointment {AppointmentId} in Google Calendar. Status: {StatusCode}, Error: {Error}", 
                    appointment.Id, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout updating appointment {AppointmentId} in Google Calendar", appointment.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId} in Google Calendar", appointment.Id);
            return false;
        }
    }

    public async Task<(bool Success, string? EventId)> CreateAppointmentInGoogleAsync(Appointment appointment, User user)
    {
        if (string.IsNullOrEmpty(user.GoogleAccessToken) || 
            string.IsNullOrEmpty(user.GoogleRefreshToken) || 
            user.GoogleTokenExpiry <= DateTime.UtcNow)
        {
            _logger.LogWarning("User {UserId} doesn't have valid Google credentials", appointment.DieticianId);
            return (false, null);
        }

        try
        {
            var accessToken = user.GoogleAccessToken;
            
            // Check if we need to refresh the token
            if (user.GoogleTokenExpiry <= DateTime.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("Access token expired or close to expiry, refresh needed. Current expiry: {Expiry}", user.GoogleTokenExpiry);
                
                // Direct refresh without using UserService to avoid DB context issues
                var newToken = await _googleAuthService.RefreshTokenDirectAsync(user.GoogleRefreshToken);
                if (newToken != null && !string.IsNullOrEmpty(newToken.AccessToken))
                {
                    accessToken = newToken.AccessToken;
                    _logger.LogInformation("Successfully refreshed token directly");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh token");
                    return (false, null);
                }
            }

            var googleEvent = CreateGoogleEvent(appointment);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var json = JsonSerializer.Serialize(googleEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                "https://www.googleapis.com/calendar/v3/calendars/primary/events", 
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var eventResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var eventId = eventResponse.GetProperty("id").GetString();
                
                _logger.LogInformation("Successfully created appointment {AppointmentId} in Google Calendar with event ID {EventId}", appointment.Id, eventId);
                return (true, eventId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create appointment {AppointmentId} in Google Calendar. Status: {StatusCode}, Error: {Error}", 
                    appointment.Id, response.StatusCode, errorContent);
                return (false, null);
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout creating appointment {AppointmentId} in Google Calendar", appointment.Id);
            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment {AppointmentId} in Google Calendar", appointment.Id);
            return (false, null);
        }
    }

    public async Task<bool> DeleteAppointmentFromGoogleAsync(Appointment appointment)
    {
        var user = await _userService.GetByIdAsync(appointment.DieticianId);
        if (user == null) return false;
        return await DeleteAppointmentFromGoogleAsync(appointment, user);
    }

    public async Task<bool> DeleteAppointmentFromGoogleAsync(Appointment appointment, User user)
    {
        if (user?.IsGoogleCalendarConnected != true) return false;
        if (string.IsNullOrEmpty(appointment.GoogleEventId)) 
        {
            _logger.LogWarning("No GoogleEventId found for appointment {AppointmentId}. Cannot delete from Google Calendar.", appointment.Id);
            return false;
        }

        try
        {
            var accessToken = await GetValidAccessTokenAsync(user);
            if (accessToken == null) return false;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.DeleteAsync(
                $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{appointment.GoogleEventId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted appointment {AppointmentId} from Google Calendar", appointment.Id);
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Appointment {AppointmentId} was already deleted from Google Calendar or not found", appointment.Id);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete appointment {AppointmentId} from Google Calendar. Status: {StatusCode}, Error: {Error}", 
                    appointment.Id, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout deleting appointment {AppointmentId} from Google Calendar", appointment.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {AppointmentId} from Google Calendar", appointment.Id);
            return false;
        }
    }

    private async Task<string?> GetValidAccessTokenAsync(User user)
    {
        return await _googleAuthService.GetValidAccessTokenAsync(user.Id);
    }

    private object CreateGoogleEvent(Appointment appointment)
    {
        return new
        {
            summary = appointment.Title,
            start = new
            {
                dateTime = appointment.Start.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                timeZone = "UTC"
            },
            end = new
            {
                dateTime = appointment.End.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                timeZone = "UTC"
            },
            description = $"VitalSense appointment - {appointment.Title}"
        };
    }
}
