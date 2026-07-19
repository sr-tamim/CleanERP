using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Identity;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Repositories.Common;

namespace CleanERP.Persistence.Repositories.Identity;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithRolesAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<User?> GetByIdWithPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<User?> GetByIdWithRolesAndPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<User>().Where(u => u.UserName == userName);
        if (excludeUserId.HasValue) query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<User>().Where(u => u.Email == email);
        if (excludeUserId.HasValue) query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        => await _context.Set<User>().AnyAsync(u => u.UserName == userName, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Set<User>().AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByUserNameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        => await _context.Set<User>()
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        => await _context.Set<User>().Where(u => u.IsActive).ToListAsync(cancellationToken);

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var directPermissions = await _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Name)
            .ToListAsync(cancellationToken);

        var rolePermissions = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(cancellationToken);

        return directPermissions.Concat(rolePermissions).Distinct();
    }
}
