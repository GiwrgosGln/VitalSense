using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("tasks")]
public class TaskItem
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; } = false;

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("dietician_id")]
    public Guid DieticianId { get; set; }

    [Column("client_id")]
    public Guid? ClientId { get; set; }
}
