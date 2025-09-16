using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class AppointmentController : ControllerBase
{
	private readonly IAppointmentService _appointmentService;

	public AppointmentController(IAppointmentService appointmentService)
	{
		_appointmentService = appointmentService;
	}

	private bool TryGetDieticianId(out Guid dieticianId)
	{
		var claim = User.FindFirst("userid")?.Value;
		return Guid.TryParse(claim, out dieticianId);
	}

	[HttpPost(ApiEndpoints.Appointments.Create)]
	[Authorize]
	[ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();

		var appt = new Appointment
		{
			Title = request.Title,
			Start = request.Start,
			End = request.End,
			AllDay = request.AllDay,
			DieticianId = dieticianId,
			ClientId = request.ClientId
		};

		var created = await _appointmentService.CreateAsync(appt);
		var response = ToResponse(created);
		return CreatedAtAction(nameof(GetById), new { appointmentId = response.Id }, response);
	}

	[HttpGet(ApiEndpoints.Appointments.GetAll)]
	[Authorize]
	[ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll()
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		var appts = await _appointmentService.GetAllByDieticianAsync(dieticianId);
		return Ok(appts.Select(ToResponse));
	}

	[HttpGet(ApiEndpoints.Appointments.GetByDate)]
	[Authorize]
	[ProducesResponseType(typeof(IEnumerable<AppointmentWithClientInfoResponse>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetByDate([FromRoute] string date)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		if (!DateOnly.TryParse(date, out var dateOnly))
		{
			return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD." });
		}
		var appts = await _appointmentService.GetAllByDieticianAndDateAsync(dieticianId, dateOnly);
		return Ok(appts);
	}

	[HttpGet(ApiEndpoints.Appointments.GetByRange)]
	[Authorize]
	[ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetByRange([FromRoute] string from, [FromRoute] string to)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
		{
			return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD for both from and to." });
		}
		var appts = await _appointmentService.GetAllByDieticianAndRangeAsync(dieticianId, fromDate, toDate);
		return Ok(appts.Select(ToResponse));
	}

	[HttpGet(ApiEndpoints.Appointments.GetById)]
	[Authorize]
	[ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> GetById([FromRoute] Guid appointmentId)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		var appt = await _appointmentService.GetByIdAsync(appointmentId);
		if (appt == null) return NotFound();
		if (appt.DieticianId != dieticianId) return Forbid();
		return Ok(ToResponse(appt));
	}

	[HttpPut(ApiEndpoints.Appointments.Edit)]
	[Authorize]
	[ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Update([FromRoute] Guid appointmentId, [FromBody] CreateAppointmentRequest request)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		var existing = await _appointmentService.GetByIdAsync(appointmentId);
		if (existing == null) return NotFound();
		if (existing.DieticianId != dieticianId) return Forbid();

		var updatedEntity = new Appointment
		{
			Title = request.Title,
			Start = request.Start,
			End = request.End,
			AllDay = request.AllDay,
			DieticianId = existing.DieticianId,
			ClientId = request.ClientId
		};
		var updated = await _appointmentService.UpdateAsync(appointmentId, updatedEntity);
		return Ok(ToResponse(updated!));
	}

	[HttpDelete(ApiEndpoints.Appointments.Delete)]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> Delete([FromRoute] Guid appointmentId)
	{
		if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
		var existing = await _appointmentService.GetByIdAsync(appointmentId);
		if (existing == null) return NotFound();
		if (existing.DieticianId != dieticianId) return Forbid();
		var ok = await _appointmentService.DeleteAsync(appointmentId);
		if (!ok) return NotFound();
		return NoContent();
	}

	private static AppointmentResponse ToResponse(Appointment a) => new()
	{
		Id = a.Id,
		Title = a.Title,
		Start = a.Start,
		End = a.End,
		AllDay = a.AllDay,
		DieticianId = a.DieticianId,
		ClientId = a.ClientId
	};
}

