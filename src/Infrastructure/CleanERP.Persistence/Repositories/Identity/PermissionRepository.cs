// This file was auto-generated - verified rewrite below
using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Identity;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Repositories.Common;

namespace CleanERP.Persistence.Repositories.Identity;

public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Set<Permission>().FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

    public async Task<bool> PermissionNameExistsAsync(string name, int? excludePermissionId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Permission>().Where(p => p.Name == name);
        if (excludePermissionId.HasValue)
            query = query.Where(p => p.Id != excludePermissionId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Permission>().Where(p => p.IsActive).OrderBy(p => p.Module).ThenBy(p => p.Action).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Permission>().Where(p => p.IsSystem).OrderBy(p => p.Module).ThenBy(p => p.Action).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
        => await _context.Set<Permission>().Where(p => p.Module == module && p.IsActive).OrderBy(p => p.Action).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Permission>> GetByModuleAndActionAsync(string module, string action, CancellationToken cancellationToken = default)
        => await _context.Set<Permission>().Where(p => p.Module == module && p.Action == action && p.IsActive).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var directPermissions = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var rolePermissions = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        return directPermissions.Concat(rolePermissions).GroupBy(p => p.Id).Select(g => g.First());
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default)
    {
        var hasDirect = await _context.Set<UserPermission>()
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == permissionName && up.Permission.IsActive, cancellationToken);
        if (hasDirect) return true;

        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId &&
                ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permissionName && rp.Permission.IsActive), cancellationToken);
    }

    public async Task<bool> UserHasModuleActionAsync(int userId, string module, string action, string? resource = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive && p.Module == module && p.Action == action);

        if (!string.IsNullOrEmpty(resource))
            query = query.Where(p => p.Resource == resource);

        if (await query.AnyAsync(cancellationToken)) return true;

        var directQuery = _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .Where(p => p.IsActive && p.Module == module && p.Action == action);

        if (!string.IsNullOrEmpty(resource))
            directQuery = directQuery.Where(p => p.Resource == resource);

        return await directQuery.AnyAsync(cancellationToken);
    }
}
