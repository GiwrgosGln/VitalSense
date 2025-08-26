using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class ChangeUsernameRequest
{
    [Required]
    [MinLength(6)]
    public string NewUsername { get; set; } = string.Empty;
    
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}
