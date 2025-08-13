using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;

namespace VitalSense.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet(ApiEndpoints.Health.Get)]
    public IActionResult Get() => Ok(new { status = "Healthy" });
}