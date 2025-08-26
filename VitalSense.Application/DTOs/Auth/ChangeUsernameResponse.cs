namespace VitalSense.Application.DTOs;

public class ChangeUsernameResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Username { get; set; }
}
