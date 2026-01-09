using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for Role entity
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Where(r => r.IsActive)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Where(r => r.IsSystem)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Set<Role>().AddAsync(role, cancellationToken);
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Set<Role>().Update(role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Set<Role>().Remove(role);
        return Task.CompletedTask;
    }

    public async Task<(IEnumerable<Role> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r => r.Name.Contains(searchTerm) || 
                                   r.DisplayName.Contains(searchTerm) ||
                                   r.Description!.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .Where(r => r.IsActive)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetUserAssignableRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Where(r => r.IsActive && !r.IsSystem)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && ur.Role.IsActive, 
                cancellationToken);
    }

    public async Task<bool> UserHasRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.Role.IsActive, 
                cancellationToken);
    }

    public async Task<int> GetRoleUserCountAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .CountAsync(ur => ur.RoleId == roleId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetRoleUsersAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User)
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task AddRolePermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTime.UtcNow
        };

        await _context.Set<RolePermission>().AddAsync(rolePermission, cancellationToken);
    }

    public async Task RemoveRolePermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission != null)
        {
            _context.Set<RolePermission>().Remove(rolePermission);
        }
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }
}
