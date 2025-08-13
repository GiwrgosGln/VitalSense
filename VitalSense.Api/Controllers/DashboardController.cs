using VitalSense.Application.DTOs;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;

namespace VitalSense.Api.Controllers;

[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private bool TryGetDieticianId(out Guid dieticianId)
    {
        var claim = User.FindFirst("userid")?.Value;
        return Guid.TryParse(claim, out dieticianId);
    }

    [HttpGet(ApiEndpoints.Dashboard.Metrics)]
    [Authorize]
    [ProducesResponseType(typeof(DashboardMetricsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var metrics = await _dashboardService.GetMetricsAsync(dieticianId, DateTime.UtcNow);
        return Ok(metrics);
    }
}
