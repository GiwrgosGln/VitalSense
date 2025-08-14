using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;

namespace VitalSense.Api.Controllers;

[ApiController]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleAuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirst("userid")?.Value;
        return Guid.TryParse(claim, out userId);
    }

    [HttpGet(ApiEndpoints.Integrations.Google.Authorize)]
    [Authorize]
    [ProducesResponseType(typeof(GoogleAuthUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGoogleAuthUrl()
    {
        try
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var response = await _googleAuthService.GetAuthorizationUrlAsync(userId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating the Google authorization URL." });
        }
    }

    [HttpPost(ApiEndpoints.Integrations.Google.Callback)]
    [ProducesResponseType(typeof(GoogleCalendarConnectionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleGoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || !Guid.TryParse(state, out var userId))
        {
            return BadRequest(new { message = "Invalid callback parameters" });
        }

        var response = await _googleAuthService.HandleOAuthCallbackAsync(code, userId);
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Integrations.Google.Status)]
    [Authorize]
    [ProducesResponseType(typeof(GoogleCalendarStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoogleCalendarStatus()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _googleAuthService.GetConnectionStatusAsync(userId);
        return Ok(response);
    }

    [HttpPost(ApiEndpoints.Integrations.Google.Disconnect)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DisconnectGoogleCalendar()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var success = await _googleAuthService.DisconnectGoogleCalendarAsync(userId);
        
        return Ok(new { success, message = success ? "Google Calendar disconnected successfully" : "Failed to disconnect Google Calendar" });
    }
}
