using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Entities.Identity;
using GoldenFiberERP.Domain.Interfaces.Repositories.Identity;
using GoldenFiberERP.Persistence.Contexts;

namespace GoldenFiberERP.Infrastructure.Persistence.Repositories.Identity;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Set<User>().AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Set<User>().Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Set<User>().Remove(user);
        return Task.CompletedTask;
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.UserName.Contains(searchTerm) || 
                                   u.Email.Contains(searchTerm) ||
                                   u.FirstName.Contains(searchTerm) ||
                                   u.LastName.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersWithPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .Where(u => u.UserPermissions.Any(up => up.Permission.Name == permission) ||
                       u.UserRoles.Any(ur => ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permission)))
            .ToListAsync(cancellationToken);
    }
}
