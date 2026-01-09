using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Common;

namespace GoldenFiberERP.Domain.Interfaces.Repositories.Identity;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAndPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}
