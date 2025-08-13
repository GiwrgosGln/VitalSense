using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("meal_days")]
public class MealDay
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("meal_plan_id")]
    public Guid MealPlanId { get; set; }

    [ForeignKey("MealPlanId")]
    public MealPlan MealPlan { get; set; } = null!;

    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}