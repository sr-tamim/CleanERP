using GoldenFiberERP.Domain.Entities.Common;

namespace GoldenFiberERP.Domain.Entities.Identity;

/// <summary>
/// Join entity for User-Permission many-to-many relationship (direct user permissions)
/// </summary>
public class UserPermission : BaseEntity
{
    public int UserId { get; private set; }
    public int PermissionId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public int? GrantedBy { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual Permission Permission { get; private set; } = null!;

    // Constructor for Entity Framework
    private UserPermission() { }

    public UserPermission(int userId, int permissionId, int? grantedBy = null)
    {
        UserId = userId;
        PermissionId = permissionId;
        GrantedAt = DateTime.UtcNow;
        GrantedBy = grantedBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
