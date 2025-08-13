using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("meal_plans")]
public class MealPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Column("dietician_id")]
    public Guid DieticianId { get; set; }

    [Required]
    [Column("client_id")]
    public Guid ClientId { get; set; }

    public ICollection<MealDay> Days { get; set; } = new List<MealDay>();

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}