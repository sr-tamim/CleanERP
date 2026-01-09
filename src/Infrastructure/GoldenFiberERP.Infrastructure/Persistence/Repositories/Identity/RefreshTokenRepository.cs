using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for RefreshToken entity
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && 
                                     rt.IsActive && 
                                     rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.IsActive && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetTokensByDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.DeviceId == deviceId)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetTokensByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.IpAddress == ipAddress)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<bool> IsActiveAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .AnyAsync(rt => rt.Token == token && rt.IsActive && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.Set<RefreshToken>().Update(refreshToken);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.Set<RefreshToken>().Remove(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.IsActive = false;
            refreshToken.RevokedAt = DateTime.UtcNow;
            _context.Set<RefreshToken>().Update(refreshToken);
        }
    }

    public async Task RevokeAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.IsActive = false;
            token.RevokedAt = DateTime.UtcNow;
        }

        _context.Set<RefreshToken>().UpdateRange(refreshTokens);
    }

    public async Task RevokeAllByDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var refreshTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.DeviceId == deviceId && rt.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.IsActive = false;
            token.RevokedAt = DateTime.UtcNow;
        }

        _context.Set<RefreshToken>().UpdateRange(refreshTokens);
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.Set<RefreshToken>().RemoveRange(expiredTokens);
    }

    public async Task<int> GetActiveTokenCountByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .CountAsync(rt => rt.UserId == userId && rt.IsActive && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<(IEnumerable<RefreshToken> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, int? userId, bool? isActive, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(rt => rt.UserId == userId.Value);
        }

        if (isActive.HasValue)
        {
            if (isActive.Value)
            {
                query = query.Where(rt => rt.IsActive && rt.ExpiresAt > DateTime.UtcNow);
            }
            else
            {
                query = query.Where(rt => !rt.IsActive || rt.ExpiresAt <= DateTime.UtcNow);
            }
        }

        if (fromDate.HasValue)
        {
            query = query.Where(rt => rt.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(rt => rt.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(rt => rt.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<RefreshToken?> GetLatestByUserAndDeviceAsync(int userId, string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId && rt.IsActive)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasActiveTokenAsync(int userId, string deviceId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .AnyAsync(rt => rt.UserId == userId && 
                           rt.DeviceId == deviceId && 
                           rt.IsActive && 
                           rt.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }
}
