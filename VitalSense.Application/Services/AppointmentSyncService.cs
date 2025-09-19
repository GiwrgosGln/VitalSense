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

        foreach (var appointmentInfo in upcomingAppointments)
        {
            try
            {
                _logger.LogInformation("Processing appointment {AppointmentId}, GoogleEventId: {GoogleEventId}", 
                    appointmentInfo.Id, appointmentInfo.GoogleEventId ?? "NULL");
                
                var appointment = new Domain.Entities.Appointment
                {
                    Id = appointmentInfo.Id,
                    Title = appointmentInfo.Title,
                    Start = appointmentInfo.Start,
                    End = appointmentInfo.End,
                    AllDay = appointmentInfo.AllDay,
                    DieticianId = appointmentInfo.DieticianId,
                    ClientId = appointmentInfo.ClientId,
                    GoogleEventId = appointmentInfo.GoogleEventId
                };
                
                _logger.LogInformation("Attempting to sync appointment {AppointmentId} with GoogleEventId: {GoogleEventId}", 
                    appointment.Id, appointment.GoogleEventId ?? "NULL");
                
                var success = await _googleCalendarService.SyncAppointmentToGoogleAsync(appointment, user);
                
                if (success)
                {
                    var existingAppointment = await _appointmentService.GetByIdAsync(appointment.Id);
                    if (existingAppointment != null && existingAppointment.GoogleEventId != appointment.GoogleEventId)
                    {
                        existingAppointment.GoogleEventId = appointment.GoogleEventId;
                        var updateResult = await _appointmentService.UpdateAsync(existingAppointment.Id, existingAppointment, true);
                        
                        if (updateResult != null)
                        {
                            syncedCount++;
                            _logger.LogInformation("Successfully synced appointment {AppointmentId} to Google Calendar with GoogleEventId: {GoogleEventId}", 
                                appointment.Id, appointment.GoogleEventId ?? "NULL");
                        }
                        else
                        {
                            failedCount++;
                            _logger.LogWarning("Failed to update appointment {AppointmentId} in database after Google Calendar sync", 
                                appointment.Id);
                        }
                    }
                    else
                    {
                        syncedCount++;
                        _logger.LogInformation("Appointment {AppointmentId} already synced with GoogleEventId: {GoogleEventId}", 
                            appointment.Id, appointment.GoogleEventId ?? "NULL");
                    }
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
                _logger.LogError(ex, "Error syncing appointment {AppointmentId} to Google Calendar. Exception: {ExceptionMessage}", 
                    appointmentInfo.Id, ex.Message);
            }
        }

        _logger.LogInformation("Completed syncing future appointments for user {UserId}. Total: {Total}, Synced: {Synced}, Failed: {Failed}", 
            userId, totalCount, syncedCount, failedCount);
            
        return (totalCount, syncedCount, failedCount);
    }
}
