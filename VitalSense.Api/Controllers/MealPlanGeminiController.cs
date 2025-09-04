using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public class MealPlanGeminiController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public MealPlanGeminiController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
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
                        
            var mealPlanRequest = await _geminiService.ConvertExcelToMealPlanAsync(
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

public class ExcelConversionRequest
{
    [FromForm]
    public IFormFile ExcelFile { get; set; } = null!;
    
    [FromForm]
    public Guid DieticianId { get; set; }
    
    [FromForm]
    public Guid ClientId { get; set; }
}