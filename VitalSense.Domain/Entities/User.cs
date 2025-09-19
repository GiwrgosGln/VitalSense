using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public byte[] PasswordHash { get; set; } = null!;

    [Required]
    [Column("password_salt")]
    public byte[] PasswordSalt { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry")]
    public DateTime? RefreshTokenExpiry { get; set; }

    // Google Calendar Integration
    [Column("google_access_token")]
    public string? GoogleAccessToken { get; set; }

    [Column("google_refresh_token")]
    public string? GoogleRefreshToken { get; set; }

    [Column("google_token_expiry")]
    public DateTime? GoogleTokenExpiry { get; set; }

    public bool IsGoogleCalendarConnected => !string.IsNullOrEmpty(GoogleAccessToken) && 
                                         !string.IsNullOrEmpty(GoogleRefreshToken) && 
                                         GoogleTokenExpiry.HasValue && 
                                         GoogleTokenExpiry > DateTime.UtcNow;
}