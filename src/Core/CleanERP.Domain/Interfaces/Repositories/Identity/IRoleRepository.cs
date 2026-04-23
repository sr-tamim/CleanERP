using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Common;

namespace CleanERP.Domain.Interfaces.Repositories.Identity;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> RoleNameExistsAsync(string name, int? excludeRoleId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
