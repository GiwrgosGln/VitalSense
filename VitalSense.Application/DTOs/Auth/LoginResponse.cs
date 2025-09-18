namespace VitalSense.Application.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime? AccessTokenExpiry { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }

    public UserDto User { get; set; } = new UserDto();
}