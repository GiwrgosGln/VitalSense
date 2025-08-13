using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VitalSense.Application.Interfaces;

namespace VitalSense.Application.Services;

public class AppointmentService : IAppointmentService
{
	private readonly AppDbContext _context;

	public AppointmentService(AppDbContext context)
	{
		_context = context;
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
		return appointment;
	}

	public async Task<Appointment?> UpdateAsync(Guid appointmentId, Appointment updated)
	{
		var existing = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
		if (existing == null) return null;

		existing.Title = updated.Title;
		existing.Start = updated.Start;
		existing.End = updated.End;
		existing.AllDay = updated.AllDay;
		existing.ClientId = updated.ClientId;

		await _context.SaveChangesAsync();
		return existing;
	}

	public async Task<bool> DeleteAsync(Guid appointmentId)
	{
		var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
		if (appt == null) return false;
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

	public async Task<IEnumerable<Appointment>> GetAllByDieticianAndDateAsync(Guid dieticianId, DateOnly date)
	{
		var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
		var endExclusive = start.AddDays(1);
		return await _context.Appointments
			.Where(a => a.DieticianId == dieticianId && a.Start >= start && a.Start < endExclusive)
			.OrderBy(a => a.Start)
			.ToListAsync();
	}

	public async Task<IEnumerable<Appointment>> GetAllByDieticianAndRangeAsync(Guid dieticianId, DateOnly from, DateOnly to)
	{
		// Inclusive of 'from' day start, inclusive of 'to' day end
		if (to < from) (from, to) = (to, from);
		var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
		var rangeEndExclusive = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
		return await _context.Appointments
			.Where(a => a.DieticianId == dieticianId && a.Start >= rangeStart && a.Start < rangeEndExclusive)
			.OrderBy(a => a.Start)
			.ToListAsync();
	}
}

