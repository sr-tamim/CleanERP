using System.ComponentModel.DataAnnotations;
using CleanERP.Domain.Entities.Common;
using CleanERP.Domain.Events.Identity;

namespace CleanERP.Domain.Entities.Identity;

/// <summary>
/// User entity representing system users
/// </summary>
public class User : AuditableEntity
{
    [Required]
    [StringLength(100)]
    public string UserName { get; private set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; private set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; private set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; private set; } = string.Empty;

    [Required]
    public string PasswordHash { get; private set; } = string.Empty;

    [StringLength(500)]
    public string? ProfilePicture { get; private set; }

    [StringLength(20)]
    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; } = false;
    public bool PhoneNumberConfirmed { get; private set; } = false;
    public bool TwoFactorEnabled { get; private set; } = false;
    public bool LockoutEnabled { get; private set; } = true;
    public DateTimeOffset? LockoutEnd { get; private set; }
    public int AccessFailedCount { get; private set; } = 0;
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<UserPermission> UserPermissions { get; private set; } = new List<UserPermission>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // Constructor for Entity Framework
    private User() { }

    public User(string userName, string email, string firstName, string lastName, string passwordHash)
    {
        UserName = userName;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;

        AddDomainEvent(new UserCreatedEvent(Id, userName, email));
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? profilePicture)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ProfilePicture = profilePicture;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(Id, firstName, lastName));
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id, UserName));
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserEmailConfirmedEvent(Id, Email));
    }

    public void ConfirmPhoneNumber()
    {
        PhoneNumberConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivatedEvent(Id, UserName));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedEvent(Id, UserName));
    }

    public void Lockout(DateTimeOffset lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserLockedOutEvent(Id, UserName, lockoutEnd));
    }

    public void RecordFailedAccess()
    {
        AccessFailedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSuccessfulLogin(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserLoggedInEvent(Id, UserName, ipAddress));
    }

    public void AssignRole(Role role)
    {
        if (UserRoles.Any(ur => ur.RoleId == role.Id))
            return;

        var userRole = new UserRole(Id, role.Id);
        UserRoles.Add(userRole);

        AddDomainEvent(new UserRoleAssignedEvent(Id, role.Id, role.Name));
    }

    public void RemoveRole(int roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            UserRoles.Remove(userRole);
            AddDomainEvent(new UserRoleRemovedEvent(Id, roleId));
        }
    }

    public void GrantPermission(Permission permission)
    {
        if (UserPermissions.Any(up => up.PermissionId == permission.Id))
            return;

        var userPermission = new UserPermission(Id, permission.Id);
        UserPermissions.Add(userPermission);

        AddDomainEvent(new UserPermissionGrantedEvent(Id, permission.Id, permission.Name));
    }

    public void RevokePermission(int permissionId)
    {
        var userPermission = UserPermissions.FirstOrDefault(up => up.PermissionId == permissionId);
        if (userPermission != null)
        {
            UserPermissions.Remove(userPermission);
            AddDomainEvent(new UserPermissionRevokedEvent(Id, permissionId));
        }
    }

    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;
    }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}".Trim();
    }
}
