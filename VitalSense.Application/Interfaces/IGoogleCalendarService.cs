using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface IGoogleCalendarService
{
    Task<bool> SyncAppointmentToGoogleAsync(Appointment appointment);
    Task<bool> SyncAppointmentToGoogleAsync(Appointment appointment, User user);
    Task<bool> UnsyncAppointmentFromGoogleAsync(Appointment appointment);
    Task<bool> UnsyncAppointmentFromGoogleAsync(Appointment appointment, User user);
    Task<bool> GoogleCalendarEventExistsAsync(string googleEventId, User user);
    Task<bool> ValidateAndCleanupStaleReferenceAsync(Appointment appointment, User user);
    Task<(bool Success, string? EventId)> CreateAppointmentInGoogleAsync(Appointment appointment, User user);
    Task<bool> UpdateAppointmentInGoogleAsync(Appointment appointment);
    Task<bool> UpdateAppointmentInGoogleAsync(Appointment appointment, User user);
    Task<bool> DeleteAppointmentFromGoogleAsync(Appointment appointment);
    Task<bool> DeleteAppointmentFromGoogleAsync(Appointment appointment, User user);
}
