using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for UserPermission entity (many-to-many relationship)
/// </summary>
public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public UserPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserPermission?> GetByIdAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Include(up => up.User)
            .Include(up => up.Permission)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);
    }

    public async Task<IEnumerable<UserPermission>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
            .OrderBy(up => up.Permission.Module)
            .ThenBy(up => up.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserPermission>> GetByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.PermissionId == permissionId)
            .Include(up => up.User)
            .OrderBy(up => up.User.FirstName)
            .ThenBy(up => up.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);
    }

    public async Task AddAsync(UserPermission userPermission, CancellationToken cancellationToken = default)
    {
        await _context.Set<UserPermission>().AddAsync(userPermission, cancellationToken);
    }

    public async Task AddAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        var userPermission = new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        };

        await _context.Set<UserPermission>().AddAsync(userPermission, cancellationToken);
    }

    public async Task DeleteAsync(int userId, int permissionId, CancellationToken cancellationToken = default)
    {
        var userPermission = await _context.Set<UserPermission>()
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);

        if (userPermission != null)
        {
            _context.Set<UserPermission>().Remove(userPermission);
        }
    }

    public Task DeleteAsync(UserPermission userPermission, CancellationToken cancellationToken = default)
    {
        _context.Set<UserPermission>().Remove(userPermission);
        return Task.CompletedTask;
    }

    public async Task DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userPermissions = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.Set<UserPermission>().RemoveRange(userPermissions);
    }

    public async Task DeleteAllByPermissionIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        var userPermissions = await _context.Set<UserPermission>()
            .Where(up => up.PermissionId == permissionId)
            .ToListAsync(cancellationToken);

        _context.Set<UserPermission>().RemoveRange(userPermissions);
    }

    public async Task<int> GetUserPermissionCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .CountAsync(up => up.UserId == userId, cancellationToken);
    }

    public async Task<int> GetPermissionUserCountAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .CountAsync(up => up.PermissionId == permissionId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.PermissionId == permissionId)
            .Select(up => up.User)
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == permissionName && up.Permission.IsActive, cancellationToken);
    }

    public async Task<bool> UserHasModuleActionAsync(int userId, string module, string action, string? resource = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserPermission>()
            .Where(up => up.UserId == userId && 
                        up.Permission.IsActive && 
                        up.Permission.Module == module && 
                        up.Permission.Action == action);

        if (!string.IsNullOrEmpty(resource))
        {
            query = query.Where(up => up.Permission.Resource == resource);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyPermissionAsync(int userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .AnyAsync(up => up.UserId == userId && permissionNames.Contains(up.Permission.Name) && up.Permission.IsActive, cancellationToken);
    }

    public async Task<bool> UserHasAllPermissionsAsync(int userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        var userPermissionNames = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId && up.Permission.IsActive)
            .Select(up => up.Permission.Name)
            .ToListAsync(cancellationToken);

        return permissionNames.All(permissionName => userPermissionNames.Contains(permissionName));
    }

    public async Task<(IEnumerable<UserPermission> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, int? userId, int? permissionId, string? module, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserPermission>()
            .Include(up => up.User)
            .Include(up => up.Permission)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(up => up.UserId == userId.Value);
        }

        if (permissionId.HasValue)
        {
            query = query.Where(up => up.PermissionId == permissionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(up => up.Permission.Module == module);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(up => up.GrantedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(up => up.GrantedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(up => up.GrantedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddMultipleAsync(int userId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        var userPermissions = permissionIds.Select(permissionId => new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        });

        await _context.Set<UserPermission>().AddRangeAsync(userPermissions, cancellationToken);
    }

    public async Task ReplaceUserPermissionsAsync(int userId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        // Remove existing permissions
        var existingPermissions = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.Set<UserPermission>().RemoveRange(existingPermissions);

        // Add new permissions
        var newUserPermissions = permissionIds.Select(permissionId => new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        });

        await _context.Set<UserPermission>().AddRangeAsync(newUserPermissions, cancellationToken);
    }

    public async Task<IEnumerable<UserPermission>> GetUserPermissionsWithDetailsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
            .Where(up => up.Permission.IsActive)
            .OrderBy(up => up.Permission.Module)
            .ThenBy(up => up.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserPermission>> GetUserPermissionsByModuleAsync(int userId, string module, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId && up.Permission.Module == module && up.Permission.IsActive)
            .Include(up => up.Permission)
            .OrderBy(up => up.Permission.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetUserModulesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId && up.Permission.IsActive)
            .Select(up => up.Permission.Module)
            .Distinct()
            .OrderBy(module => module)
            .ToListAsync(cancellationToken);
    }
}
