using Microsoft.EntityFrameworkCore;
using VitalSense.Domain.Entities;
using VitalSense.Infrastructure.Data;
using VitalSense.Application.Interfaces;

namespace VitalSense.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<bool> UpdateGoogleTokensAsync(Guid userId, string accessToken, string refreshToken, DateTime expiry)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.GoogleAccessToken = accessToken;
        user.GoogleRefreshToken = refreshToken;
        user.GoogleTokenExpiry = expiry;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearGoogleTokensAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.GoogleAccessToken = null;
        user.GoogleRefreshToken = null;
        user.GoogleTokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }
}
