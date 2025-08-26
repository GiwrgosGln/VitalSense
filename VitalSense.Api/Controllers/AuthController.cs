using Microsoft.AspNetCore.Mvc;
using VitalSense.Application.DTOs;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using VitalSense.Api.Endpoints;


namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost(ApiEndpoints.Users.Login)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(response);
    }

    [HttpPost(ApiEndpoints.Users.Register)]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.RegisterAsync(request);

        if (response == null)
        {
            return BadRequest(new { message = "Registration failed. Username or email may already be taken, or password/username is invalid." });
        }

        return Created(string.Empty, response);
    }

    [HttpPost(ApiEndpoints.Users.RefreshToken)]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.RefreshTokenAsync(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Users.Me)]
    [Authorize]
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsResponse>> GetMe()
    {
        var userId = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var userDetails = await _authService.GetUserDetailsAsync(userGuid);
        if (userDetails == null)
        {
            return NotFound("User not found");
        }

        return Ok(userDetails);
    }
    
    [HttpPost(ApiEndpoints.Users.ChangeEmail)]
    [Authorize]
    [ProducesResponseType(typeof(ChangeEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }
        
        var response = await _authService.ChangeEmailAsync(userGuid, request);
        
        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }
        
        return Ok(response);
    }
    
    [HttpPost(ApiEndpoints.Users.ChangePassword)]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }
        
        var response = await _authService.ChangePasswordAsync(userGuid, request);
        
        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }
        
        return Ok(response);
    }
    
    [HttpPost(ApiEndpoints.Users.ChangeUsername)]
    [Authorize]
    [ProducesResponseType(typeof(ChangeUsernameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }
        
        var response = await _authService.ChangeUsernameAsync(userGuid, request);
        
        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }
        
        return Ok(response);
    }
}