using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VitalSense.Application.DTOs;

public class ClientResponse
{
    public Guid Id { get; set; }
    public Guid DieticianId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool HasCard { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}