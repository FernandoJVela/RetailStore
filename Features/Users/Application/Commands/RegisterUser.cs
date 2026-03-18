using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record RegisterUserCommand(
    string Username, string Email, string Password
) : ICommand<Guid>;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _users;

    public RegisterUserHandler(IUserRepository users)
    { _users = users; }

    public async Task<Guid> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var emailExists = await _users.ExistsWithEmailAsync(
           new Email(cmd.Email), ct);
        if (emailExists)
            throw new DomainException(UserErrors.DuplicateEmail(cmd.Email));

        var usernameExists = await _users.ExistsWithUsernameAsync(
            cmd.Username, ct);
        if (usernameExists)
            throw new DomainException(UserErrors.DuplicateUsername(cmd.Username));

        var user = User.Register(cmd.Username, cmd.Email, cmd.Password);
        await _users.AddAsync(user, ct);
        return user.Id;
    }
}