using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Api.Features.Users.Infrastructure;
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
    private UserRepository _userRepository;

    public DeactivateUserHandler(UserRepository userRepository) => 
        _userRepository = userRepository;

    public async Task<Unit> Handle(DeactivateUserCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        user.Deactivate();
        // UnitOfWorkBehavior saves -> UserRoleAssignedEvent -> Outbox
        // -> PermissionCacheInvalidationHandler invalidates cache
        return Unit.Value;
    }
}
