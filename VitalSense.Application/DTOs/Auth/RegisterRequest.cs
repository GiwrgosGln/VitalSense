using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class RegisterRequest
{
    [Required]
    [MinLength(6, ErrorMessage = "Username must be at least 6 characters long.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}