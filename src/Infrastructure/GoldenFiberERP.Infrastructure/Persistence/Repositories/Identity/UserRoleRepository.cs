using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for UserRole entity (many-to-many relationship)
/// </summary>
public class UserRoleRepository : IUserRoleRepository
{
    private readonly ApplicationDbContext _context;

    public UserRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserRole?> GetByIdAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .OrderBy(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .Include(ur => ur.User)
            .OrderBy(ur => ur.User.FirstName)
            .ThenBy(ur => ur.User.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _context.Set<UserRole>().AddAsync(userRole, cancellationToken);
    }

    public async Task AddAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };

        await _context.Set<UserRole>().AddAsync(userRole, cancellationToken);
    }

    public async Task DeleteAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (userRole != null)
        {
            _context.Set<UserRole>().Remove(userRole);
        }
    }

    public Task DeleteAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        _context.Set<UserRole>().Remove(userRole);
        return Task.CompletedTask;
    }

    public async Task DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.Set<UserRole>().RemoveRange(userRoles);
    }

    public async Task DeleteAllByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _context.Set<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.Set<UserRole>().RemoveRange(userRoles);
    }

    public async Task<int> GetUserRoleCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .CountAsync(ur => ur.UserId == userId, cancellationToken);
    }

    public async Task<int> GetRoleUserCountAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .CountAsync(ur => ur.RoleId == roleId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User)
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && ur.Role.IsActive, cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && roleNames.Contains(ur.Role.Name) && ur.Role.IsActive, cancellationToken);
    }

    public async Task<bool> UserHasAllRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        var userRoleNames = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        return roleNames.All(roleName => userRoleNames.Contains(roleName));
    }

    public async Task<(IEnumerable<UserRole> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, int? userId, int? roleId, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<UserRole>()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(ur => ur.UserId == userId.Value);
        }

        if (roleId.HasValue)
        {
            query = query.Where(ur => ur.RoleId == roleId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(ur => ur.AssignedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(ur => ur.AssignedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(ur => ur.AssignedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddMultipleAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        var userRoles = roleIds.Select(roleId => new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.Set<UserRole>().AddRangeAsync(userRoles, cancellationToken);
    }

    public async Task ReplaceUserRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        // Remove existing roles
        var existingRoles = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.Set<UserRole>().RemoveRange(existingRoles);

        // Add new roles
        var newUserRoles = roleIds.Select(roleId => new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.Set<UserRole>().AddRangeAsync(newUserRoles, cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesWithDetailsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .OrderBy(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }
}
