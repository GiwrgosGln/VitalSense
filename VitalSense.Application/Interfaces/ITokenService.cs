using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
}