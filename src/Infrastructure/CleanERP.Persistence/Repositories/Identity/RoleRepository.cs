using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Interfaces.Repositories.Identity;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Repositories.Common;

namespace CleanERP.Persistence.Repositories.Identity;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Set<Role>().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public async Task<Role?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        => await _context.Set<Role>().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

    public async Task<bool> RoleNameExistsAsync(string name, int? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Role>().Where(r => r.Name == name);
        if (excludeRoleId.HasValue)
            query = query.Where(r => r.Id != excludeRoleId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Role>().Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Role>().Where(r => r.IsSystem).OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        => await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
}
