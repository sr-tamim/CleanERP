using GoldenFiberERP.Domain.Entities.Common;

namespace GoldenFiberERP.Domain.Entities.Identity;

/// <summary>
/// Join entity for Role-Permission many-to-many relationship
/// </summary>
public class RolePermission : BaseEntity
{
    public int RoleId { get; private set; }
    public int PermissionId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public int? GrantedBy { get; private set; }

    // Navigation properties
    public virtual Role Role { get; private set; } = null!;
    public virtual Permission Permission { get; private set; } = null!;

    // Constructor for Entity Framework
    private RolePermission() { }

    public RolePermission(int roleId, int permissionId, int? grantedBy = null)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAt = DateTime.UtcNow;
        GrantedBy = grantedBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
