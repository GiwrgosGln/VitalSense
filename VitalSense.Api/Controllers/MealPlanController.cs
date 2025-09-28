using VitalSense.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.Services;
using VitalSense.Application.Interfaces;
using VitalSense.Api.Models;

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

    [Authorize]
    [HttpPost(ApiEndpoints.MealPlans.ConvertExcel)]
    [ProducesResponseType(typeof(CreateMealPlanRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ConvertExcelToMealPlan([FromForm] ExcelConversionRequest request)
    {
        if (request.ExcelFile == null || request.ExcelFile.Length == 0)
            return BadRequest("No Excel file provided.");

        if (request.ExcelFile.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds the maximum limit of 10MB.");

        if (request.ExcelFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" &&
            request.ExcelFile.ContentType != "application/vnd.ms-excel")
            return BadRequest("Invalid file format. Please upload an Excel file.");

        try
        {
            using var memoryStream = new MemoryStream();
            await request.ExcelFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var excelData = memoryStream.ToArray();
                        
            var mealPlanRequest = await _mealPlanService.ConvertExcelToMealPlanAsync(
                excelData, 
                request.ExcelFile.FileName, 
                request.DieticianId, 
                request.ClientId);

            return Ok(mealPlanRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing Excel file: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error processing Excel file", details = ex.Message });
        }
    }
}