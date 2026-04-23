using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Identity;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Repositories.Common;

namespace CleanERP.Persistence.Repositories.Identity;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Set<RefreshToken>().Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Set<RefreshToken>()
            .ToListAsync(cancellationToken);

    public async Task RevokeAllUserTokensAsync(int userId, string? revokedByIp = null, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.Set<RefreshToken>()
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke(revokedByIp);
        }
    }

    public async Task RevokeTokenAsync(string token, string? revokedByIp = null, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        if (refreshToken != null)
        {
            refreshToken.Revoke(revokedByIp, replacedByToken);
        }
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked)
            .ToListAsync(cancellationToken);
        _context.Set<RefreshToken>().RemoveRange(expiredTokens);
    }
}
