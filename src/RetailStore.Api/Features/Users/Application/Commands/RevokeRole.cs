using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record RevokeRoleCommand(
    Guid UserId, Guid RoleId
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "users:manage";
    public string AuditModule => "Users";
    public string? AuditDescription => $"Revoking role {RoleId}";
}

public sealed class RevokeRoleHandler : IRequestHandler<RevokeRoleCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IRepository<Role> _roles;

    public RevokeRoleHandler(IUserRepository users, IRepository<Role> roles) 
    { 
        _users = users;
        _roles = roles;
    }

    public async Task<Unit> Handle(RevokeRoleCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        var role = await _roles.GetByIdAsync(cmd.RoleId, ct)
            ?? throw new DomainException(UserErrors.RoleNotFound(cmd.RoleId));

        user.RevokeRole(role.Id, role.Name);
        // UnitOfWorkBehavior saves -> UserRoleRevokedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
