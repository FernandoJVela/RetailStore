using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record AssignRoleCommand(
    Guid UserId, string RoleName
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "users:manage";
}

public sealed class AssignRoleHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly RetailStoreDbContext _db;

    public AssignRoleHandler(RetailStoreDbContext db) => _db = db;

    public async Task<Unit> Handle(AssignRoleCommand cmd, CancellationToken ct)
    {
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        var role = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == cmd.RoleName, ct)
            ?? throw new DomainException(UserErrors.RoleNotFound(cmd.RoleName));

        user.AssignRole(role.Id, role.Name);
        // UnitOfWorkBehavior saves -> UserRoleAssignedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
