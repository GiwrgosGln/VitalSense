using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace VitalSense.Application.Services;

public class MealPlanService : IMealPlanService
{
    private readonly AppDbContext _context;

    public MealPlanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MealPlanResponse> CreateAsync(CreateMealPlanRequest request)
    {
        var now = DateTime.UtcNow;
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DieticianId = request.DieticianId,
            ClientId = request.ClientId,
            Days = request.Days.Select(day => new MealDay
            {
                Id = Guid.NewGuid(),
                Title = day.Title,
                Meals = day.Meals.Select(meal => new Meal
                {
                    Id = Guid.NewGuid(),
                    Title = meal.Title,
                    Time = meal.Time ?? "",
                    Description = meal.Description,
                    Protein = meal.Protein,
                    Carbs = meal.Carbs,
                    Fats = meal.Fats,
                    Calories = meal.Calories
                }).ToList()
            }).ToList(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.MealPlans.Add(mealPlan);
        await _context.SaveChangesAsync();

        return new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<MealPlanResponse?> GetByIdAsync(Guid mealPlanId)
    {
        var mealPlan = await _context.MealPlans
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

        if (mealPlan == null) return null;

        return new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<List<MealPlanResponse>> GetAllAsync(Guid clientId)
    {
        var mealPlans = await _context.MealPlans
            .Where(mp => mp.ClientId == clientId)
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .ToListAsync();

        return mealPlans.Select(mealPlan => new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        }).ToList();
    }

    public async Task<MealPlanResponse?> GetActiveMealPlanAsync(Guid clientId)
    {
        var now = DateTime.UtcNow.Date;
        var latestMealPlan = await _context.MealPlans
            .Where(mp => mp.ClientId == clientId && mp.StartDate.Date <= now && mp.EndDate.Date >= now)
            .OrderByDescending(mp => mp.UpdatedAt)
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .FirstOrDefaultAsync();

        if (latestMealPlan == null) return null;

        return new MealPlanResponse
        {
            Id = latestMealPlan.Id,
            Title = latestMealPlan.Title,
            StartDate = latestMealPlan.StartDate,
            EndDate = latestMealPlan.EndDate,
            DieticianId = latestMealPlan.DieticianId,
            ClientId = latestMealPlan.ClientId,
            Days = latestMealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<MealPlanResponse?> UpdateAsync(Guid mealPlanId, UpdateMealPlanRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var existingMealPlan = await _context.MealPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);
                
            if (existingMealPlan == null)
                return null;
                
            var mealsSql = $"DELETE FROM meals WHERE meal_day_id IN (SELECT id FROM meal_days WHERE meal_plan_id = '{mealPlanId}')";
            await _context.Database.ExecuteSqlRawAsync(mealsSql);
            
            var daysSql = $"DELETE FROM meal_days WHERE meal_plan_id = '{mealPlanId}'";
            await _context.Database.ExecuteSqlRawAsync(daysSql);
            
            var updatedMealPlan = await _context.MealPlans.FindAsync(mealPlanId);
            if (updatedMealPlan == null)
            {
                await transaction.RollbackAsync();
                return null;
            }
            
            updatedMealPlan.Title = request.Title;
            updatedMealPlan.StartDate = request.StartDate;
            updatedMealPlan.EndDate = request.EndDate;
            updatedMealPlan.DieticianId = request.DieticianId;
            updatedMealPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            foreach (var dayRequest in request.Days)
            {
                var day = new MealDay
                {
                    Id = dayRequest.Id ?? Guid.NewGuid(),
                    MealPlanId = mealPlanId,
                    Title = dayRequest.Title,
                };
                
                _context.MealDays.Add(day);
                await _context.SaveChangesAsync();
                
                foreach (var mealRequest in dayRequest.Meals)
                {
                    var meal = new Meal
                    {
                        Id = mealRequest.Id ?? Guid.NewGuid(),
                        MealDayId = day.Id,
                        Title = mealRequest.Title,
                        Time = mealRequest.Time ?? "",
                        Description = mealRequest.Description,
                        Protein = mealRequest.Protein,
                        Carbs = mealRequest.Carbs,
                        Fats = mealRequest.Fats,
                        Calories = mealRequest.Calories
                    };
                    
                    _context.Meals.Add(meal);
                }
                
                await _context.SaveChangesAsync();
            }
            
            await transaction.CommitAsync();
            
            var result = await _context.MealPlans
                .Include(mp => mp.Days)
                    .ThenInclude(d => d.Meals)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);
                
            if (result == null)
                return null;
                
            return new MealPlanResponse
            {
                Id = result.Id,
                Title = result.Title,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                DieticianId = result.DieticianId,
                ClientId = result.ClientId,
                Days = result.Days.Select(d => new MealDayResponse
                {
                    Id = d.Id,
                    Title = d.Title,
                    Meals = d.Meals.Select(m => new MealResponse
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Time = m.Time,
                        Description = m.Description,
                        Protein = m.Protein,
                        Carbs = m.Carbs,
                        Fats = m.Fats,
                        Calories = m.Calories
                    }).ToList()
                }).ToList()
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}