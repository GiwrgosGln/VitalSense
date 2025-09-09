using VitalSense.Application.DTOs;

namespace VitalSense.Application.Interfaces;

public interface IMealPlanService
{
    Task<List<MealPlanResponse>> GetAllAsync(Guid clientId);
    Task<MealPlanResponse> CreateAsync(CreateMealPlanRequest request);
    Task<MealPlanResponse?> GetByIdAsync(Guid mealPlanId);
    Task<MealPlanResponse?> GetActiveMealPlanAsync(Guid clientId);
    Task<MealPlanResponse?> UpdateAsync(Guid mealPlanId, UpdateMealPlanRequest request);
    Task DeleteAsync(Guid mealPlanId);
}