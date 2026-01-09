using GoldenFiberERP.Domain.Entities.Identity;

namespace GoldenFiberERP.Domain.Interfaces.Repositories.Identity;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, bool? isActive,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersWithPermissionAsync(string permission, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Role> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, bool? isActive,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Permission entity
/// </summary>
public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
    Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
    Task DeleteAsync(Permission permission, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Permission> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? module, bool? isActive,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> UserHasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);
    Task<bool> UserHasModuleActionAsync(int userId, string module, string action, string? resource = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for RefreshToken entity
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(int userId, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task<bool> IsTokenActiveAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserRole entity
/// </summary>
public interface IUserRoleRepository
{
    Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserRole userRole, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task DeleteByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserPermission entity
/// </summary>
public interface IUserPermissionRepository
{
    Task<UserPermission?> GetByUserAndPermissionAsync(int userId, int permissionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserPermission>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserPermission>> GetByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default);
    Task AddAsync(UserPermission userPermission, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserPermission userPermission, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task DeleteByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for RolePermission entity
/// </summary>
public interface IRolePermissionRepository
{
    Task<RolePermission?> GetByRoleAndPermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default);
    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
    Task DeleteAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
    Task DeleteByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task DeleteByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default);
}
