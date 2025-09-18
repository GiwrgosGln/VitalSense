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
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
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
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            },
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
            RefreshTokenExpiry = user.RefreshTokenExpiry,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
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
    
    public async Task<ChangeEmailResponse> ChangeEmailAsync(Guid userId, ChangeEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new ChangeEmailResponse
            {
                Success = false,
                Message = "User not found."
            };
        }
        
        // Verify current password
        if (!_passwordService.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return new ChangeEmailResponse
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }
        
        // Check if email is already taken
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.NewEmail.ToLower() && u.Id != userId))
        {
            return new ChangeEmailResponse
            {
                Success = false,
                Message = "Email already in use."
            };
        }
        
        // Update email
        user.Email = request.NewEmail;
        await _context.SaveChangesAsync();
        
        return new ChangeEmailResponse
        {
            Success = true,
            Message = "Email successfully updated.",
            Email = user.Email
        };
    }
    
    public async Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "User not found."
            };
        }
        
        // Verify current password
        if (!_passwordService.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }
        
        // Create new password hash
        _passwordService.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
        
        // Update password
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        
        // Invalidate refresh token to force re-login with new credentials
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        
        await _context.SaveChangesAsync();
        
        return new ChangePasswordResponse
        {
            Success = true,
            Message = "Password successfully updated."
        };
    }
    
    public async Task<ChangeUsernameResponse> ChangeUsernameAsync(Guid userId, ChangeUsernameRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new ChangeUsernameResponse
            {
                Success = false,
                Message = "User not found."
            };
        }
        
        // Verify current password
        if (!_passwordService.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return new ChangeUsernameResponse
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }
        
        // Validate new username
        if (string.IsNullOrEmpty(request.NewUsername) || request.NewUsername.Length < 6)
        {
            return new ChangeUsernameResponse
            {
                Success = false,
                Message = "Username must be at least 6 characters."
            };
        }
        
        // Check if username is already taken
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.NewUsername.ToLower() && u.Id != userId))
        {
            return new ChangeUsernameResponse
            {
                Success = false,
                Message = "Username already taken."
            };
        }
        
        // Update username
        user.Username = request.NewUsername;
        await _context.SaveChangesAsync();
        
        return new ChangeUsernameResponse
        {
            Success = true,
            Message = "Username successfully updated.",
            Username = user.Username
        };
    }
}