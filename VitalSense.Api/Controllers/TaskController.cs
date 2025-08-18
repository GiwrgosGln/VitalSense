using VitalSense.Application.DTOs;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private bool TryGetDieticianId(out Guid dieticianId)
    {
        var claim = User.FindFirst("userid")?.Value;
        return Guid.TryParse(claim, out dieticianId);
    }

    [HttpPost(ApiEndpoints.Tasks.Create)]
    [Authorize]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var created = await _taskService.CreateAsync(dieticianId, request);
        return CreatedAtAction(nameof(GetById), new { taskId = created.Id }, created);
    }

    [HttpGet(ApiEndpoints.Tasks.GetAll)]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var tasks = await _taskService.GetAllAsync(dieticianId);
        return Ok(tasks);
    }

    [HttpGet(ApiEndpoints.Tasks.GetById)]
    [Authorize]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid taskId)
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var task = await _taskService.GetByIdAsync(dieticianId, taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost(ApiEndpoints.Tasks.ToggleComplete)]
    [Authorize]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleComplete([FromRoute] Guid taskId)
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var task = await _taskService.ToggleCompleteAsync(dieticianId, taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete(ApiEndpoints.Tasks.Delete)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid taskId)
    {
        if (!TryGetDieticianId(out var dieticianId)) return Unauthorized();
        var ok = await _taskService.DeleteAsync(dieticianId, taskId);
        if (!ok) return NotFound();
        return NoContent();
    }
}
