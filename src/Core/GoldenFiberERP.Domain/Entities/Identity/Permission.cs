using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;

namespace GoldenFiberERP.Domain.Entities.Identity;

/// <summary>
/// Permission entity representing system permissions
/// </summary>
public class Permission : AuditableEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; private set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; private set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; private set; }

    [Required]
    [StringLength(100)]
    public string Module { get; private set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; private set; } = string.Empty; // Create, Read, Update, Delete, Execute

    [StringLength(100)]
    public string? Resource { get; private set; } // Specific resource like "Products", "Orders"

    public bool IsActive { get; private set; } = true;
    public bool IsSystem { get; private set; } = false;

    // Navigation properties
    public virtual ICollection<UserPermission> UserPermissions { get; private set; } = new List<UserPermission>();
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Constructor for Entity Framework
    private Permission() { }

    public Permission(string name, string displayName, string module, string action, string? resource = null, string? description = null, bool isSystem = false)
    {
        Name = name;
        DisplayName = displayName;
        Module = module;
        Action = action;
        Resource = resource;
        Description = description;
        IsSystem = isSystem;
    }

    public void UpdateDetails(string displayName, string? description)
    {
        DisplayName = displayName;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("System permissions cannot be deactivated");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFullPermissionName()
    {
        return Resource != null ? $"{Module}.{Resource}.{Action}" : $"{Module}.{Action}";
    }
}
