using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for Permission entity
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetSystemPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .Where(p => p.IsSystem)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .Where(p => p.Module == module && p.IsActive)
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .AnyAsync(p => p.Name == name, cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Set<Permission>().AddAsync(permission, cancellationToken);
    }

    public Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Set<Permission>().Update(permission);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Set<Permission>().Remove(permission);
        return Task.CompletedTask;
    }

    public async Task<(IEnumerable<Permission> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? module, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Permission>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   p.DisplayName.Contains(searchTerm) ||
                                   p.Description!.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(p => p.Module == module);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Get permissions directly assigned to user
        var userPermissions = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        // Get permissions from user roles
        var rolePermissions = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        // Combine and deduplicate
        var allPermissions = userPermissions.Concat(rolePermissions)
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action);

        return allPermissions;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        // Check direct user permissions
        var hasDirectPermission = await _context.Set<UserPermission>()
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == permission && up.Permission.IsActive, 
                cancellationToken);

        if (hasDirectPermission)
            return true;

        // Check role permissions
        var hasRolePermission = await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && 
                           ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permission && rp.Permission.IsActive),
                cancellationToken);

        return hasRolePermission;
    }

    public async Task<bool> UserHasModuleActionAsync(int userId, string module, string action, string? resource = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive && p.Module == module && p.Action == action);

        if (!string.IsNullOrEmpty(resource))
        {
            query = query.Where(p => p.Resource == resource);
        }

        var hasRolePermission = await query.AnyAsync(cancellationToken);

        if (hasRolePermission)
            return true;

        // Check direct user permissions
        var userQuery = _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .Where(p => p.IsActive && p.Module == module && p.Action == action);

        if (!string.IsNullOrEmpty(resource))
        {
            userQuery = userQuery.Where(p => p.Resource == resource);
        }

        return await userQuery.AnyAsync(cancellationToken);
    }
}
