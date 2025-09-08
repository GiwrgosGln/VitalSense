using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface IAppointmentSyncService
{
    Task<(int Total, int Synced, int Failed)> SyncAllFutureAppointmentsAsync(Guid userId);
}
