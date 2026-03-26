using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record DeactivateUserCommand(
    Guid UserId
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "users:manage";
    public string AuditModule => "Users";
    public string? AuditDescription => $"Deactivating user";
}

public sealed class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private IUserRepository _users;

    public DeactivateUserHandler(IUserRepository userRepository) => 
        _users = userRepository;

    public async Task<Unit> Handle(DeactivateUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        user.Deactivate();
        // UnitOfWorkBehavior saves -> UserRoleAssignedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
