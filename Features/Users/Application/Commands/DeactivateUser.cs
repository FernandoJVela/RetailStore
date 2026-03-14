using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record DeactivateUserCommand(
    Guid UserId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "users:manage";
}

public sealed class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly RetailStoreDbContext _db;

    public DeactivateUserHandler(RetailStoreDbContext db) => _db = db;

    public async Task<Unit> Handle(DeactivateUserCommand cmd, CancellationToken ct)
    {
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        user.Deactivate();
        // UnitOfWorkBehavior saves -> UserRoleAssignedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
