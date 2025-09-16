using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VitalSense.Application.Interfaces;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Services;

public class AppointmentService : IAppointmentService
{
	private readonly AppDbContext _context;
	private readonly IGoogleCalendarService _googleCalendarService;

	public AppointmentService(AppDbContext context, IGoogleCalendarService googleCalendarService)
	{
		_context = context;
		_googleCalendarService = googleCalendarService;
	}

	public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
		=> await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);

	public async Task<Appointment> CreateAsync(Appointment appointment)
	{
		if (appointment.Id == Guid.Empty)
		{
			appointment.Id = Guid.NewGuid();
		}

		await _context.Appointments.AddAsync(appointment);
		await _context.SaveChangesAsync();

		// Check if the user has Google Calendar connected and sync the appointment
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == appointment.DieticianId);
		if (user?.IsGoogleCalendarConnected == true)
		{
			await SyncToGoogleCalendarAndSaveEventIdAsync(appointment, user);
		}

		return appointment;
	}

	private async Task SyncToGoogleCalendarAndSaveEventIdAsync(Appointment appointment, Domain.Entities.User? user)
	{
		try
		{
			await Task.Delay(500);
			
			// Check if user has Google Calendar connected
			if (user?.IsGoogleCalendarConnected != true)
			{
				Console.WriteLine($"INFO: Google Calendar not connected for user {appointment.DieticianId}, skipping sync for appointment {appointment.Id}");
				return;
			}

			var result = await _googleCalendarService.CreateAppointmentInGoogleAsync(appointment, user);
			if (result.Success && !string.IsNullOrEmpty(result.EventId))
			{
				Console.WriteLine($"SUCCESS: Google Calendar sync completed for appointment {appointment.Id} with event ID {result.EventId}");
				
				// Save the Google Event ID to the database
				appointment.GoogleEventId = result.EventId;
				_context.Appointments.Update(appointment);
				await _context.SaveChangesAsync();
				
				Console.WriteLine($"SUCCESS: Google Event ID {result.EventId} saved for appointment {appointment.Id}");
			}
			else
			{
				Console.WriteLine("WARNING: Google Calendar sync returned false for appointment " + appointment.Id);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"ERROR: Google Calendar sync failed for appointment {appointment.Id}");
			Console.WriteLine($"Exception Type: {ex.GetType().Name}");
			Console.WriteLine($"Message: {ex.Message}");
			if (ex.InnerException != null)
			{
				Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
			}
			Console.WriteLine($"Stack Trace: {ex.StackTrace}");
		}
	}

	public async Task<Appointment?> UpdateAsync(Guid appointmentId, Appointment updated)
	{
		var existing = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
		if (existing == null) return null;

		// Store the GoogleEventId before updating other fields
		var googleEventId = existing.GoogleEventId;

		existing.Title = updated.Title;
		existing.Start = updated.Start;
		existing.End = updated.End;
		existing.AllDay = updated.AllDay;
		existing.ClientId = updated.ClientId;
		// Keep the original GoogleEventId
		existing.GoogleEventId = googleEventId;

		await _context.SaveChangesAsync();

		// Get user data BEFORE starting background task (while context is still active)
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existing.DieticianId);

		// Sync with Google Calendar (but don't wait for it to complete)
		_ = UpdateGoogleCalendarAsync(existing, user);

		return existing;
	}

	private async Task UpdateGoogleCalendarAsync(Appointment appointment, User? user)
	{
		try
		{
			if (user != null)
			{
				await _googleCalendarService.UpdateAppointmentInGoogleAsync(appointment, user);
				Console.WriteLine($"SUCCESS: Google Calendar sync completed for appointment {appointment.Id}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"ERROR: Failed to sync appointment {appointment.Id} with Google Calendar: {ex.Message}");
		}
	}

	private async Task DeleteFromGoogleCalendarAsync(Appointment appointment, User? user)
	{
		try
		{
			if (user != null)
			{
				await _googleCalendarService.DeleteAppointmentFromGoogleAsync(appointment, user);
				Console.WriteLine($"SUCCESS: Google Calendar deletion completed for appointment {appointment.Id}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"ERROR: Failed to delete appointment {appointment.Id} from Google Calendar: {ex.Message}");
		}
	}

	public async Task<bool> DeleteAsync(Guid appointmentId)
	{
		var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
		if (appt == null) return false;

		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == appt.DieticianId);

		if (user != null)
		{
			try
			{
				var googleDeleted = await _googleCalendarService.DeleteAppointmentFromGoogleAsync(appt, user);
				if (!googleDeleted)
				{
					Console.WriteLine($"WARNING: Could not delete appointment {appt.Id} from Google Calendar");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: Exception deleting appointment {appt.Id} from Google Calendar: {ex.Message}");
			}
		}

		_context.Appointments.Remove(appt);
		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<IEnumerable<Appointment>> GetAllByDieticianAsync(Guid dieticianId)
	{
		return await _context.Appointments
			.Where(a => a.DieticianId == dieticianId)
			.OrderBy(a => a.Start)
			.ToListAsync();
	}

	// Helper method to get appointments with client information
	public async Task<IEnumerable<AppointmentWithClientInfoResponse>> GetAllByDieticianAndDateAsync(Guid dieticianId, DateOnly date)
	{
		var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
		var endExclusive = start.AddDays(1);
		return await _context.Appointments
			.Where(a => a.DieticianId == dieticianId && a.Start >= start && a.Start < endExclusive)
			.Join(
				_context.Clients,
				appointment => appointment.ClientId,
				client => client.Id,
				(appointment, client) => new AppointmentWithClientInfoResponse
				{
					Id = appointment.Id,
					Title = appointment.Title,
					Start = appointment.Start,
					End = appointment.End,
					AllDay = appointment.AllDay,
					DieticianId = appointment.DieticianId,
					ClientId = appointment.ClientId,
					ClientFirstName = client.FirstName,
					ClientLastName = client.LastName,
					ClientEmail = client.Email,
					ClientPhone = client.Phone
				})
			.OrderBy(a => a.Start)
			.ToListAsync();
	}

	public async Task<IEnumerable<Appointment>> GetAllByDieticianAndRangeAsync(Guid dieticianId, DateOnly from, DateOnly to)
	{
		if (to < from) (from, to) = (to, from);
		var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
		var rangeEndExclusive = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
		return await _context.Appointments
			.Where(a => a.DieticianId == dieticianId && a.Start >= rangeStart && a.Start < rangeEndExclusive)
			.OrderBy(a => a.Start)
			.ToListAsync();
	}
}

