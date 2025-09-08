using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class UpdateMealPlanRequest
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
    public List<MealDayRequest> Days { get; set; } = new();
}
