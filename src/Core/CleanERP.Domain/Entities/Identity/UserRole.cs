using CleanERP.Domain.Entities.Common;

namespace CleanERP.Domain.Entities.Identity;

/// <summary>
/// Join entity for User-Role many-to-many relationship
/// </summary>
public class UserRole : BaseEntity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public int? AssignedBy { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual Role Role { get; private set; } = null!;

    // Constructor for Entity Framework
    private UserRole() { }

    public UserRole(int userId, int roleId, int? assignedBy = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
