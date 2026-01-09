using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Common;

namespace GoldenFiberERP.Domain.Interfaces.Repositories.Identity;

public interface IPermissionRepository : IBaseRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> PermissionNameExistsAsync(string name, int? excludePermissionId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByModuleAndActionAsync(string module, string action, CancellationToken cancellationToken = default);
}
