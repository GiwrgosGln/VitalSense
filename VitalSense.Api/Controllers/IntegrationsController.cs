using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.Interfaces;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class GoogleCalendarController : ControllerBase
{
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IAppointmentService _appointmentService;
    private readonly IUserService _userService;
    private readonly IAppointmentSyncService _appointmentSyncService;

    public GoogleCalendarController(
        IGoogleCalendarService googleCalendarService,
        IAppointmentService appointmentService,
        IUserService userService,
        IAppointmentSyncService appointmentSyncService)
    {
        _googleCalendarService = googleCalendarService;
        _appointmentService = appointmentService;
        _userService = userService;
        _appointmentSyncService = appointmentSyncService;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirst("userid")?.Value;
        return Guid.TryParse(claim, out userId);
    }

    [HttpPost(ApiEndpoints.Integrations.GoogleCalendar.SyncAppointment)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncAppointmentToGoogle(Guid appointmentId)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        // Verify the appointment belongs to the authenticated user
        if (appointment.DieticianId != userId)
        {
            return Forbid();
        }

        // Store the original GoogleEventId to check if it was created or updated
        var originalGoogleEventId = appointment.GoogleEventId;
        
        var success = await _googleCalendarService.SyncAppointmentToGoogleAsync(appointment);
        
        // If sync was successful and a new GoogleEventId was assigned, update the database
        if (success && string.IsNullOrEmpty(originalGoogleEventId) && !string.IsNullOrEmpty(appointment.GoogleEventId))
        {
            await _appointmentService.UpdateAsync(appointmentId, appointment);
        }
        
        var message = success 
            ? (string.IsNullOrEmpty(originalGoogleEventId) 
                ? "Appointment synced to Google Calendar successfully" 
                : "Appointment updated in Google Calendar successfully")
            : "Failed to sync appointment to Google Calendar";
        
        return Ok(new { 
            success, 
            message,
            isAlreadySynced = !string.IsNullOrEmpty(originalGoogleEventId)
        });
    }

    [HttpPost(ApiEndpoints.Integrations.GoogleCalendar.UnsyncAppointment)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsyncAppointmentFromGoogle(Guid appointmentId)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        // Verify the appointment belongs to the authenticated user
        if (appointment.DieticianId != userId)
        {
            return Forbid();
        }

        // Check if appointment is currently synced
        var wassynced = !string.IsNullOrEmpty(appointment.GoogleEventId);
        
        var success = await _googleCalendarService.UnsyncAppointmentFromGoogleAsync(appointment);
        
        // If unsync was successful and the appointment was previously synced, update the database
        if (success && wassynced && string.IsNullOrEmpty(appointment.GoogleEventId))
        {
            await _appointmentService.UpdateAsync(appointmentId, appointment);
        }
        
        var message = success 
            ? (wassynced 
                ? "Appointment unsynced from Google Calendar successfully" 
                : "Appointment was not synced to Google Calendar")
            : "Failed to unsync appointment from Google Calendar";
        
        return Ok(new { 
            success, 
            message,
            wasSynced = wassynced
        });
    }

    [HttpPost(ApiEndpoints.Integrations.GoogleCalendar.ValidateAppointment)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateAppointmentSync(Guid appointmentId)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        // Verify the appointment belongs to the authenticated user
        if (appointment.DieticianId != userId)
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(appointment.GoogleEventId))
        {
            return Ok(new { 
                isValid = true,
                isSynced = false,
                message = "Appointment is not synced to Google Calendar" 
            });
        }

        // Get user for Google Calendar access
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return Ok(new { 
                isValid = false,
                isSynced = true,
                message = "Unable to verify sync status - user not found" 
            });
        }

        // Check if the Google Calendar event still exists
        var eventExists = await _googleCalendarService.GoogleCalendarEventExistsAsync(appointment.GoogleEventId, user);
        
        // If event doesn't exist, clean up the stale reference
        var cleanupPerformed = false;
        if (!eventExists && !string.IsNullOrEmpty(appointment.GoogleEventId))
        {
            appointment.GoogleEventId = null;
            await _appointmentService.UpdateAsync(appointmentId, appointment);
            cleanupPerformed = true;
        }

        return Ok(new { 
            isValid = eventExists,
            isSynced = !string.IsNullOrEmpty(appointment.GoogleEventId),
            cleanupPerformed,
            message = eventExists 
                ? "Appointment sync is valid" 
                : (cleanupPerformed 
                    ? "Stale sync reference cleaned up - event was deleted from Google Calendar" 
                    : "Event no longer exists in Google Calendar")
        });
    }

    [HttpPost(ApiEndpoints.Integrations.GoogleCalendar.SyncAllFutureAppointments)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncAllFutureAppointments()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        // Get user to check if Google Calendar is connected
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!user.IsGoogleCalendarConnected)
        {
            return BadRequest(new { 
                success = false, 
                message = "Google Calendar is not connected. Please connect Google Calendar first." 
            });
        }

        var (total, synced, failed) = await _appointmentSyncService.SyncAllFutureAppointmentsAsync(userId);

        return Ok(new { 
            success = failed == 0,
            message = $"Synced {synced} of {total} future appointments to Google Calendar",
            details = new {
                totalAppointments = total,
                syncedSuccessfully = synced,
                syncFailed = failed
            }
        });
    }
}
