using VitalSense.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class MealPlanController : ControllerBase
{
    private readonly IMealPlanService _mealPlanService;

    public MealPlanController(IMealPlanService mealPlanService)
    {
        _mealPlanService = mealPlanService;
    }

    [Authorize]
    [HttpPost(ApiEndpoints.MealPlans.Create)]
    [ProducesResponseType(typeof(MealPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMealPlanRequest request)
    {
        var result = await _mealPlanService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { mealPlanId = result.Id }, result);
    }

    [Authorize]
    [HttpGet(ApiEndpoints.MealPlans.GetById)]
    [ProducesResponseType(typeof(MealPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid mealPlanId)
    {
        var mealPlan = await _mealPlanService.GetByIdAsync(mealPlanId);
        if (mealPlan == null)
            return NotFound();
        return Ok(mealPlan);
    }

    [Authorize]
    [HttpGet(ApiEndpoints.MealPlans.GetByClientId)]
    [ProducesResponseType(typeof(List<MealPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMealPlansByClientId([FromRoute] Guid clientId)
    {
        var mealPlans = await _mealPlanService.GetAllAsync(clientId);
        return Ok(mealPlans);
    }

    [HttpGet(ApiEndpoints.MealPlans.GetActiveByClientId)]
    [ProducesResponseType(typeof(MealPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveMealPlan([FromRoute] Guid clientId)
    {
        var mealPlan = await _mealPlanService.GetActiveMealPlanAsync(clientId);
        if (mealPlan == null)
            return NotFound();
        return Ok(mealPlan);
    }

    [Authorize]
    [HttpPut(ApiEndpoints.MealPlans.Edit)]
    [ProducesResponseType(typeof(MealPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Edit([FromRoute] Guid mealPlanId, [FromBody] UpdateMealPlanRequest request)
    {
        var updatedMealPlan = await _mealPlanService.UpdateAsync(mealPlanId, request);
        if (updatedMealPlan == null)
            return NotFound();
        return Ok(updatedMealPlan);
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.MealPlans.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid mealPlanId)
    {
        var existingMealPlan = await _mealPlanService.GetByIdAsync(mealPlanId);
        if (existingMealPlan == null)
            return NotFound();

        await _mealPlanService.DeleteAsync(mealPlanId);
        return NoContent();
    }
}