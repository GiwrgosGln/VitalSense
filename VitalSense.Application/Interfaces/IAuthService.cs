using VitalSense.Application.DTOs;

namespace VitalSense.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserDetailsResponse?> GetUserDetailsAsync(Guid userId);
    Task<ChangeEmailResponse> ChangeEmailAsync(Guid userId, ChangeEmailRequest request);
    Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<ChangeUsernameResponse> ChangeUsernameAsync(Guid userId, ChangeUsernameRequest request);
}