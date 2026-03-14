using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Api.Features.Users.Infrastructure;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record LoginCommand(
    string Email, string Password
) : ICommand<LoginResponse>;

public sealed record LoginResponse(
    string AccessToken, string RefreshToken,
    DateTime ExpiresAt, Guid UserId, string Username);

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    { RuleFor(x => x.Email).NotEmpty(); RuleFor(x => x.Password).NotEmpty(); }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly RetailStoreDbContext _db;
    private readonly IJwtTokenService _jwt;

    public LoginHandler(RetailStoreDbContext db, IJwtTokenService jwt)
    { _db = db; _jwt = jwt; }

    public async Task<LoginResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == new Email(cmd.Email), ct)
            ?? throw new DomainException(UserErrors.InvalidCredentials());

        user.ValidateCredentials(cmd.Password);  // Throws if invalid

        var tokens = await _jwt.GenerateTokenPairAsync(user, ct);

        // Store hashed refresh token on the user aggregate
        var refreshHash = Convert.ToBase64String(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(tokens.RefreshToken)));
        user.SetRefreshToken(refreshHash, TimeSpan.FromDays(7));

        return new LoginResponse(
            tokens.AccessToken, tokens.RefreshToken,
            tokens.ExpiresAt, user.Id, user.Username);
    }
}