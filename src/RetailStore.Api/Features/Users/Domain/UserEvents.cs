using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Domain;

// ─── User lifecycle ─────────────────────────────────────────
public sealed record UserRegisteredEvent(
    Guid UserId, string Username, string Email) : DomainEvent
{ public override string EventType => "UserRegistered"; }

public sealed record UserLoggedInEvent(
    Guid UserId, DateTime LoginAt) : DomainEvent
{ public override string EventType => "UserLoggedIn"; }

public sealed record UserDeactivatedEvent(
    Guid UserId) : DomainEvent
{ public override string EventType => "UserDeactivated"; }

public sealed record UserReactivatedEvent(
    Guid UserId) : DomainEvent
{ public override string EventType => "UserReactivated"; }

public sealed record UserPasswordChangedEvent(
    Guid UserId) : DomainEvent
{ public override string EventType => "UserPasswordChanged"; }

// ─── Role/permission changes (trigger cache invalidation) ──
public sealed record UserRoleAssignedEvent(
    Guid UserId, string RoleName) : DomainEvent
{ public override string EventType => "UserRoleAssigned"; }

public sealed record UserRoleRevokedEvent(
    Guid UserId, string RoleName) : DomainEvent
{ public override string EventType => "UserRoleRevoked"; }

public sealed record RolePermissionsChangedEvent(
    Guid RoleId, string RoleName,
    List<string> AddedPermissions,
    List<string> RemovedPermissions) : DomainEvent
{ public override string EventType => "RolePermissionsChanged"; }

public sealed record RefreshTokenRotatedEvent(
    Guid UserId, string OldTokenHash) : DomainEvent
{ public override string EventType => "RefreshTokenRotated"; }