using Microsoft.Extensions.Logging;
using VitalSense.Application.Interfaces;
using VitalSense.Domain.Entities;

namespace VitalSense.Application.Services;

public class AppointmentSyncService : IAppointmentSyncService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IUserService _userService;
    private readonly ILogger<AppointmentSyncService> _logger;

    public AppointmentSyncService(
        IAppointmentService appointmentService,
        IGoogleCalendarService googleCalendarService,
        IUserService userService,
        ILogger<AppointmentSyncService> logger)
    {
        _appointmentService = appointmentService;
        _googleCalendarService = googleCalendarService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<(int Total, int Synced, int Failed)> SyncAllFutureAppointmentsAsync(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null || !user.IsGoogleCalendarConnected)
        {
            _logger.LogWarning("User {UserId} not found or not connected to Google Calendar", userId);
            return (0, 0, 0);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureAppointments = await _appointmentService.GetAllByDieticianAndRangeAsync(
            userId, 
            today, 
            today.AddMonths(6)
        );

        var now = DateTime.UtcNow;
        var upcomingAppointments = futureAppointments
            .Where(a => a.Start > now)
            .ToList();

        if (!upcomingAppointments.Any())
        {
            _logger.LogInformation("No future appointments found for user {UserId}", userId);
            return (0, 0, 0);
        }

        int totalCount = upcomingAppointments.Count;
        int syncedCount = 0;
        int failedCount = 0;

        _logger.LogInformation("Syncing {Count} future appointments for user {UserId}", totalCount, userId);

        foreach (var appointment in upcomingAppointments)
        {
            try
            {
                var success = await _googleCalendarService.SyncAppointmentToGoogleAsync(appointment, user);
                
                if (success)
                {
                    if (appointment.GoogleEventId != null)
                    {
                        await _appointmentService.UpdateAsync(appointment.Id, appointment);
                    }
                    
                    syncedCount++;
                    _logger.LogInformation("Successfully synced appointment {AppointmentId} to Google Calendar", appointment.Id);
                }
                else
                {
                    failedCount++;
                    _logger.LogWarning("Failed to sync appointment {AppointmentId} to Google Calendar", appointment.Id);
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                _logger.LogError(ex, "Error syncing appointment {AppointmentId} to Google Calendar", appointment.Id);
            }
        }

        _logger.LogInformation("Completed syncing future appointments for user {UserId}. Total: {Total}, Synced: {Synced}, Failed: {Failed}", 
            userId, totalCount, syncedCount, failedCount);
            
        return (totalCount, syncedCount, failedCount);
    }
}
