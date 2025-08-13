using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("meals")]
public class Meal
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Column("time")]
    public string Time { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("protein")]
    public float Protein { get; set; }

    [Column("carbs")]
    public float Carbs { get; set; }

    [Column("fats")]
    public float Fats { get; set; }

    [Column("calories")]
    public float Calories { get; set; }

    [Required]
    [Column("meal_day_id")]
    public Guid MealDayId { get; set; }

    [ForeignKey("MealDayId")]
    public MealDay MealDay { get; set; } = null!;
}