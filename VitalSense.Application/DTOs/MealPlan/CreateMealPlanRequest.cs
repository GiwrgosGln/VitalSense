using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class CreateMealPlanRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public Guid DieticianId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public List<MealDayRequest> Days { get; set; } = new();
}

public class MealDayRequest
{
    public Guid? Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public List<MealRequest> Meals { get; set; } = new();
}

public class MealRequest
{
    public Guid? Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Time { get; set; }
    public string? Description { get; set; }
    public float Protein { get; set; }
    public float Carbs { get; set; }
    public float Fats { get; set; }
    public float Calories { get; set; }
}