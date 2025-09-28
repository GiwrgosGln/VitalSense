using VitalSense.Domain.Entities;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using VitalSense.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

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
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 20)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var clients = await _clientService.GetAllByDieticianAsync(dieticianId, pageNumber, pageSize);
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
        
        return Ok(clients);
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

        request.DieticianId = dieticianId;
        request.CreatedAt = DateTime.UtcNow;

        var created = await _clientService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { clientId = created.Id }, created);
    }

    [HttpPut(ApiEndpoints.Clients.Edit)]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid clientId, [FromBody] UpdateClientRequest request)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
            return Unauthorized();

        var existing = await _clientService.GetByIdAsync(clientId);
        if (existing == null) return NotFound();
        if (existing.DieticianId != dieticianId) return Forbid();

        var updated = await _clientService.UpdateAsync(clientId, request);
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
    
    [HttpPost(ApiEndpoints.Clients.ImportClients)]
    [Authorize]
    [ProducesResponseType(typeof(ImportClientsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportClients(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
            return BadRequest("Excel file is required.");

        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }
        
        try
        {
            using var stream = excelFile.OpenReadStream();
            var result = await _clientService.ImportFromExcelAsync(dieticianId, stream);
            
            if (result.SuccessCount == 0 && result.Errors.Any())
            {
                return BadRequest(new { message = "Import failed", errors = result.Errors });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error processing Excel file", error = ex.Message });
        }
    }
}