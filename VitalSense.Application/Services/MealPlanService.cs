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
}