using MediatR;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record AssignRoleCommand(
    Guid UserId, Guid RoleId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "users:manage";
}

public sealed class AssignRoleHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IRepository<Role> _roles;

    public AssignRoleHandler(IUserRepository users, IRepository<Role> roles) => (_users, _roles) = (users, roles);

    public async Task<Unit> Handle(AssignRoleCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        var role = await _roles.GetByIdAsync(cmd.RoleId, ct)
            ?? throw new DomainException(UserErrors.RoleNotFound(cmd.RoleId));

        user.AssignRole(role.Id, role.Name);
        // UnitOfWorkBehavior saves -> UserRoleAssignedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
