using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record ReactivateUserCommand(
    Guid UserId
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "users:manage";
    public string AuditModule => "Users";
    public string? AuditDescription => $"Reactivating user";
}

public sealed class ReactivateUserHandler : IRequestHandler<ReactivateUserCommand, Unit>
{
    private readonly IUserRepository _users;

    public ReactivateUserHandler(IUserRepository userRepository) =>
        _users = userRepository;

    public async Task<Unit> Handle(ReactivateUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException(UserErrors.NotFound(cmd.UserId));

        user.Reactivate();
        return Unit.Value;
    }
}
