using VitalSense.Application.DTOs;

namespace VitalSense.Application.Interfaces;

public interface IGeminiService
{
    Task<CreateMealPlanRequest> ConvertExcelToMealPlanAsync(byte[] excelFileData, string fileName, Guid dieticianId, Guid clientId);
}
