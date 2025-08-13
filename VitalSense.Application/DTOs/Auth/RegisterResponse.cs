namespace VitalSense.Application.DTOs;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime? AccessTokenExpiry { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }
}