using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Common;

namespace CleanERP.Domain.Interfaces.Repositories.Identity;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(int userId, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, string? revokedByIp = null, string? replacedByToken = null, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
