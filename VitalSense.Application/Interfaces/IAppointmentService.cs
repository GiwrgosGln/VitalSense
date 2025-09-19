using VitalSense.Application.DTOs;
using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface IAppointmentService
{
    Task<Appointment?> GetByIdAsync(Guid appointmentId);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment?> UpdateAsync(Guid appointmentId, Appointment appointment, bool skipGoogleSync = false);
    Task<bool> DeleteAsync(Guid appointmentId);
    Task<IEnumerable<Appointment>> GetAllByDieticianAsync(Guid dieticianId);
    Task<IEnumerable<AppointmentWithClientInfoResponse>> GetAllByDieticianAndDateAsync(Guid dieticianId, DateOnly date);
    Task<IEnumerable<AppointmentWithClientInfoResponse>> GetAllByDieticianAndRangeAsync(Guid dieticianId, DateOnly from, DateOnly to);
}