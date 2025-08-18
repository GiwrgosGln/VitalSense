using VitalSense.Domain.Entities;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using VitalSense.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using Microsoft.AspNetCore.Authorization;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet(ApiEndpoints.Clients.GetAll)]
    [ProducesResponseType(typeof(IEnumerable<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var clients = await _clientService.GetAllByDieticianAsync(dieticianId);
        return Ok(clients);
    }

    [HttpGet(ApiEndpoints.Clients.Search)]
    [ProducesResponseType(typeof(IEnumerable<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }
        if (limit <= 0 || limit > 100) limit = 20;
        var clients = await _clientService.SearchAsync(dieticianId, q, limit);
        return Ok(clients.Select(c => new ClientResponse
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            DateOfBirth = c.DateOfBirth,
            Gender = c.Gender,
            HasCard = c.HasCard,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }));
    }

    [HttpGet(ApiEndpoints.Clients.GetById)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById([FromRoute] Guid clientId)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var client = await _clientService.GetByIdAsync(clientId);
        if (client == null) return NotFound();

        if (client.DieticianId != dieticianId)
            return Forbid();

        return Ok(client);
    }

    [HttpPost(ApiEndpoints.Clients.Create)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            HasCard = request.HasCard,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            DieticianId = dieticianId
        };

        var created = await _clientService.CreateAsync(client);

        var response = new ClientResponse
        {
            Id = created.Id,
            FirstName = created.FirstName,
            LastName = created.LastName,
            Email = created.Email,
            Phone = created.Phone,
            DateOfBirth = created.DateOfBirth,
            Gender = created.Gender,
            HasCard = created.HasCard,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { clientId = response.Id }, response);
    }

    [HttpPut(ApiEndpoints.Clients.Edit)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid clientId, [FromBody] Client client)
    {
        var updated = await _clientService.UpdateAsync(clientId, client);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete(ApiEndpoints.Clients.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid clientId)
    {
        var deleted = await _clientService.DeleteAsync(clientId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}