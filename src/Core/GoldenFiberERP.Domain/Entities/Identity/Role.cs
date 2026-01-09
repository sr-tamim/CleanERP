using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;
using GoldenFiberERP.Domain.Events.Identity;

namespace GoldenFiberERP.Domain.Entities.Identity;

/// <summary>
/// Role entity representing user roles in the system
/// </summary>
public class Role : AuditableEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; private set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsSystem { get; private set; } = false;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Constructor for Entity Framework
    private Role() { }

    public Role(string name, string? description = null, bool isSystem = false)
    {
        Name = name;
        Description = description;
        IsSystem = isSystem;

        AddDomainEvent(new RoleCreatedEvent(Id, name));
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RoleUpdatedEvent(Id, name));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("System roles cannot be deactivated");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void GrantPermission(Permission permission)
    {
        if (RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        var rolePermission = new RolePermission(Id, permission.Id);
        RolePermissions.Add(rolePermission);

        AddDomainEvent(new RolePermissionGrantedEvent(Id, permission.Id, Name, permission.Name));
    }

    public void RevokePermission(int permissionId)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            RolePermissions.Remove(rolePermission);
            AddDomainEvent(new RolePermissionRevokedEvent(Id, permissionId, Name));
        }
    }

    public IEnumerable<Permission> GetPermissions()
    {
        return RolePermissions.Select(rp => rp.Permission).Where(p => p != null)!;
    }
}
