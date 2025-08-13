using Microsoft.EntityFrameworkCore;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using VitalSense.Infrastructure.Data;
using VitalSense.Application.Interfaces;

namespace VitalSense.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext context,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u =>
            u.Username.ToLower() == request.Username.ToLower());

        if (user == null)
        {
            return null;
        }

        if (!_passwordService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        user.LastLogin = DateTime.UtcNow;

        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            AccessTokenExpiry = _tokenService.GetAccessTokenExpiry(),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = user.RefreshTokenExpiry,
            UserId = user.Id,
            Username = user.Username,
        };
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || request.Username.Length < 6)
        {
            return new RegisterResponse { Success = false, Message = "Username too short." };
        }

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
        {
            return new RegisterResponse { Success = false, Message = "Username already taken." };
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            return new RegisterResponse { Success = false, Message = "Email already taken." };
        }

        _passwordService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return new RegisterResponse
        {
            Success = true,
            Message = "Registration successful.",
            UserId = user.Id,
            Username = user.Username,
            AccessToken = _tokenService.GenerateAccessToken(user),
            AccessTokenExpiry = _tokenService.GetAccessTokenExpiry(),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = user.RefreshTokenExpiry
        };
    }

    public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return null;
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();
        await _context.SaveChangesAsync();

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            AccessTokenExpiry = _tokenService.GetAccessTokenExpiry(),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = user.RefreshTokenExpiry
        };
    }

    public async Task<UserDetailsResponse?> GetUserDetailsAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        return new UserDetailsResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }
}