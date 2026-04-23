using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Common;

namespace CleanERP.Domain.Interfaces.Repositories.Identity;

public interface IPermissionRepository : IBaseRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> PermissionNameExistsAsync(string name, int? excludePermissionId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByModuleAndActionAsync(string module, string action, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UserHasPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default);
    Task<bool> UserHasModuleActionAsync(int userId, string module, string action, string? resource = null, CancellationToken cancellationToken = default);
}
