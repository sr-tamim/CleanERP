using GoldenFiberERP.Domain.Events;

namespace GoldenFiberERP.Domain.Events.Identity;

public record UserCreatedEvent(int UserId, string UserName, string Email) : IDomainEvent;

public record UserProfileUpdatedEvent(int UserId, string FirstName, string LastName) : IDomainEvent;

public record UserPasswordChangedEvent(int UserId, string UserName) : IDomainEvent;

public record UserEmailConfirmedEvent(int UserId, string Email) : IDomainEvent;

public record UserActivatedEvent(int UserId, string UserName) : IDomainEvent;

public record UserDeactivatedEvent(int UserId, string UserName) : IDomainEvent;

public record UserLockedOutEvent(int UserId, string UserName, DateTimeOffset LockoutEnd) : IDomainEvent;

public record UserLoggedInEvent(int UserId, string UserName, string IpAddress) : IDomainEvent;

public record UserRoleAssignedEvent(int UserId, int RoleId, string RoleName) : IDomainEvent;

public record UserRoleRemovedEvent(int UserId, int RoleId) : IDomainEvent;

public record UserPermissionGrantedEvent(int UserId, int PermissionId, string PermissionName) : IDomainEvent;

public record UserPermissionRevokedEvent(int UserId, int PermissionId) : IDomainEvent;

public record RoleCreatedEvent(int RoleId, string RoleName) : IDomainEvent;

public record RoleUpdatedEvent(int RoleId, string RoleName) : IDomainEvent;

public record RolePermissionGrantedEvent(int RoleId, int PermissionId, string RoleName, string PermissionName) : IDomainEvent;

public record RolePermissionRevokedEvent(int RoleId, int PermissionId, string RoleName) : IDomainEvent;
