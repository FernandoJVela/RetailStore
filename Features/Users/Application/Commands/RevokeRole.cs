using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record RevokeRoleCommand(
    Guid UserId, string RoleName
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "users:manage";
}

public sealed class RevokeRoleHandler : IRequestHandler<RevokeRoleCommand, Unit>
{
    private readonly RetailStoreDbContext _db;

    public RevokeRoleHandler(RetailStoreDbContext db) => _db = db;

    public async Task<Unit> Handle(RevokeRoleCommand cmd, CancellationToken ct)
    {
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        var role = await _db.Set<Role>().FirstOrDefaultAsync(r => r.Name == cmd.RoleName, ct)
            ?? throw new DomainException(UserErrors.RoleNotFound(cmd.RoleName));

        user.RevokeRole(role.Id, role.Name);
        // UnitOfWorkBehavior saves -> UserRoleRevokedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
