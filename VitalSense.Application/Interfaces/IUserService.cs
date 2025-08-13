using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<bool> UpdateGoogleTokensAsync(Guid userId, string accessToken, string refreshToken, DateTime expiry);
    Task<bool> ClearGoogleTokensAsync(Guid userId);
}
