namespace VitalSense.Application.DTOs;

public class ChangeEmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Email { get; set; }
}
