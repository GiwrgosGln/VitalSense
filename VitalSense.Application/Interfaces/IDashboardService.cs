using VitalSense.Application.DTOs;

namespace VitalSense.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardMetricsResponse> GetMetricsAsync(Guid dieticianId, DateTime utcNow);
}
