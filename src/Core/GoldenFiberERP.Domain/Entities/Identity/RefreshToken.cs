using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;

namespace GoldenFiberERP.Domain.Entities.Identity;

/// <summary>
/// Refresh token entity for secure token management
/// </summary>
public class RefreshToken : BaseEntity
{
    [Required]
    public string Token { get; private set; } = string.Empty;

    [Required]
    public int UserId { get; private set; }

    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? CreatedByIp { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;

    // Constructor for Entity Framework
    private RefreshToken() { }

    public RefreshToken(string token, int userId, DateTime expiresAt, string? createdByIp = null)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
        UpdatedAt = DateTime.UtcNow;
    }
}
