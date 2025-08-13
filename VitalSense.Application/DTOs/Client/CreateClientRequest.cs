using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class CreateClientRequest
{
    [Required]
    [MinLength(4, ErrorMessage = "First Name must be at least 6 characters long.")]
    [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public required string FirstName { get; set; }

    [Required]
    [MinLength(4, ErrorMessage = "Last Name must be at least 6 characters long.")]
    [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool HasCard { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}