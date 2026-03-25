using MediatR;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Users.Application.Events;

/// <summary>
/// Listens to all permission-related domain events and invalidates
/// the appropriate permission cache entries. This handler runs
/// asynchronously via the Outbox processor.
/// </summary>
public sealed class PermissionCacheInvalidationHandler :
    INotificationHandler<UserRoleAssignedEvent>,
    INotificationHandler<UserRoleRevokedEvent>,
    INotificationHandler<RolePermissionsChangedEvent>,
    INotificationHandler<UserDeactivatedEvent>,
    INotificationHandler<UserReactivatedEvent>
{
    private readonly IPermissionService _permissions;
    private readonly ILogger<PermissionCacheInvalidationHandler> _log;

    public PermissionCacheInvalidationHandler(
        IPermissionService permissions,
        ILogger<PermissionCacheInvalidationHandler> log)
    { _permissions = permissions; _log = log; }

    public Task Handle(UserRoleAssignedEvent e, CancellationToken ct)
    {
        _log.LogInformation("Role '{Role}' assigned to user {UserId} → invalidating cache",
            e.RoleName, e.UserId);
        _permissions.InvalidateCache(e.UserId);
        return Task.CompletedTask;
    }

    public Task Handle(UserRoleRevokedEvent e, CancellationToken ct)
    {
        _log.LogInformation("Role '{Role}' revoked from user {UserId} → invalidating cache",
            e.RoleName, e.UserId);
        _permissions.InvalidateCache(e.UserId);
        return Task.CompletedTask;
    }

    public Task Handle(RolePermissionsChangedEvent e, CancellationToken ct)
    {
        _log.LogWarning(
            "Permissions changed on role '{Role}' (+{Added}/-{Removed}) → invalidating ALL caches",
            e.RoleName, e.AddedPermissions.Count, e.RemovedPermissions.Count);
        _permissions.InvalidateCacheForRole(e.RoleId);
        return Task.CompletedTask;
    }

    public Task Handle(UserDeactivatedEvent e, CancellationToken ct)
    {
        _permissions.InvalidateCache(e.UserId);
        return Task.CompletedTask;
    }

    public Task Handle(UserReactivatedEvent e, CancellationToken ct)
    {
        _permissions.InvalidateCache(e.UserId);
        return Task.CompletedTask;
    }
}