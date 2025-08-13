namespace VitalSense.Application.DTOs;

public class MealPlanResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid DieticianId { get; set; }
    public Guid ClientId { get; set; }
    public List<MealDayResponse> Days { get; set; } = new();
}

public class MealDayResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<MealResponse> Meals { get; set; } = new();
}

public class MealResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Time { get; set; }
    public string? Description { get; set; }
    public float Protein { get; set; }
    public float Carbs { get; set; }
    public float Fats { get; set; }
    public float Calories { get; set; }
}