using CleanERP.Domain.Events;

namespace CleanERP.Domain.Events.Identity;

public record UserCreatedEvent(int UserId, string UserName, string Email) : DomainEvent;

public record UserProfileUpdatedEvent(int UserId, string FirstName, string LastName) : DomainEvent;

public record UserPasswordChangedEvent(int UserId, string UserName) : DomainEvent;

public record UserEmailConfirmedEvent(int UserId, string Email) : DomainEvent;

public record UserActivatedEvent(int UserId, string UserName) : DomainEvent;

public record UserDeactivatedEvent(int UserId, string UserName) : DomainEvent;

public record UserLockedOutEvent(int UserId, string UserName, DateTimeOffset LockoutEnd) : DomainEvent;

public record UserLoggedInEvent(int UserId, string UserName, string IpAddress) : DomainEvent;

public record UserRoleAssignedEvent(int UserId, int RoleId, string RoleName) : DomainEvent;

public record UserRoleRemovedEvent(int UserId, int RoleId) : DomainEvent;

public record UserPermissionGrantedEvent(int UserId, int PermissionId, string PermissionName) : DomainEvent;

public record UserPermissionRevokedEvent(int UserId, int PermissionId) : DomainEvent;

public record RoleCreatedEvent(int RoleId, string RoleName) : DomainEvent;

public record RoleUpdatedEvent(int RoleId, string RoleName) : DomainEvent;

public record RolePermissionGrantedEvent(int RoleId, int PermissionId, string RoleName, string PermissionName) : DomainEvent;

public record RolePermissionRevokedEvent(int RoleId, int PermissionId, string RoleName) : DomainEvent;
