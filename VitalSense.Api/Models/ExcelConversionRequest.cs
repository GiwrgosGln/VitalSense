using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VitalSense.Api.Models;

public class ExcelConversionRequest
{
    [FromForm]
    public IFormFile ExcelFile { get; set; } = null!;
    
    [FromForm]
    public Guid DieticianId { get; set; }
    
    [FromForm]
    public Guid ClientId { get; set; }
}