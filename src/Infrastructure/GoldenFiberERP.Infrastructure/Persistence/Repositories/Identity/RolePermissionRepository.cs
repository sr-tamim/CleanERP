using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for RolePermission entity (many-to-many relationship)
/// </summary>
public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RolePermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RolePermission?> GetByIdAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .OrderBy(rp => rp.Permission.Module)
            .ThenBy(rp => rp.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.PermissionId == permissionId)
            .Include(rp => rp.Role)
            .OrderBy(rp => rp.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        await _context.Set<RolePermission>().AddAsync(rolePermission, cancellationToken);
    }

    public async Task AddAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        };

        await _context.Set<RolePermission>().AddAsync(rolePermission, cancellationToken);
    }

    public async Task DeleteAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission != null)
        {
            _context.Set<RolePermission>().Remove(rolePermission);
        }
    }

    public Task DeleteAsync(RolePermission rolePermission, CancellationToken cancellationToken = default)
    {
        _context.Set<RolePermission>().Remove(rolePermission);
        return Task.CompletedTask;
    }

    public async Task DeleteAllByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.Set<RolePermission>().RemoveRange(rolePermissions);
    }

    public async Task DeleteAllByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _context.Set<RolePermission>()
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync(cancellationToken);

        _context.Set<RolePermission>().RemoveRange(rolePermissions);
    }

    public async Task<int> GetRolePermissionCountAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .CountAsync(rp => rp.RoleId == roleId, cancellationToken);
    }

    public async Task<int> GetPermissionRoleCountAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .CountAsync(rp => rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.PermissionId == permissionId)
            .Select(rp => rp.Role)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, string permissionName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permissionName && rp.Permission.IsActive, cancellationToken);
    }

    public async Task<bool> RoleHasModuleActionAsync(int roleId, string module, string action, string? resource = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId && 
                        rp.Permission.IsActive && 
                        rp.Permission.Module == module && 
                        rp.Permission.Action == action);

        if (!string.IsNullOrEmpty(resource))
        {
            query = query.Where(rp => rp.Permission.Resource == resource);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> RoleHasAnyPermissionAsync(int roleId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && permissionNames.Contains(rp.Permission.Name) && rp.Permission.IsActive, cancellationToken);
    }

    public async Task<bool> RoleHasAllPermissionsAsync(int roleId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        var rolePermissionNames = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(cancellationToken);

        return permissionNames.All(permissionName => rolePermissionNames.Contains(permissionName));
    }

    public async Task<(IEnumerable<RolePermission> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, int? roleId, int? permissionId, string? module, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<RolePermission>()
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AsQueryable();

        if (roleId.HasValue)
        {
            query = query.Where(rp => rp.RoleId == roleId.Value);
        }

        if (permissionId.HasValue)
        {
            query = query.Where(rp => rp.PermissionId == permissionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(rp => rp.Permission.Module == module);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(rp => rp.GrantedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(rp => rp.GrantedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(rp => rp.GrantedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddMultipleAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        var rolePermissions = permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        });

        await _context.Set<RolePermission>().AddRangeAsync(rolePermissions, cancellationToken);
    }

    public async Task ReplaceRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        // Remove existing permissions
        var existingPermissions = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.Set<RolePermission>().RemoveRange(existingPermissions);

        // Add new permissions
        var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        });

        await _context.Set<RolePermission>().AddRangeAsync(newRolePermissions, cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissionsWithDetailsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Where(rp => rp.Permission.IsActive)
            .OrderBy(rp => rp.Permission.Module)
            .ThenBy(rp => rp.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissionsByModuleAsync(int roleId, string module, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId && rp.Permission.Module == module && rp.Permission.IsActive)
            .Include(rp => rp.Permission)
            .OrderBy(rp => rp.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetRoleModulesAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Module)
            .Distinct()
            .OrderBy(module => module)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RolePermission>> GetEffectivePermissionsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.Permission.IsActive)
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .OrderBy(rp => rp.Permission.Module)
            .ThenBy(rp => rp.Permission.Action)
            .ToListAsync(cancellationToken);
    }
}
