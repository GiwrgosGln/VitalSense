using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("clients")]
public class Client
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("phone")]
    public string Phone { get; set; } = null!;

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("has_card")]
    public bool HasCard { get; set; } = false;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("dietician_id")]
    public Guid DieticianId { get; set; }
}