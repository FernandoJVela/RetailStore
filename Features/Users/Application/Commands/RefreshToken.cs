using System.Security.Claims;
using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Api.Features.Users.Infrastructure;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Application.Commands;

public sealed record RefreshTokenCommand(
    string AccessToken, string RefreshToken
) : ICommand<LoginResponse>;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly RetailStoreDbContext _db;
    private readonly IJwtTokenService _jwt;

    public RefreshTokenHandler(RetailStoreDbContext db, IJwtTokenService jwt)
    { _db = db; _jwt = jwt; }

    public async Task<LoginResponse> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        // Validate expired access token to extract user ID
        var principal = _jwt.ValidateExpiredToken(cmd.AccessToken)
            ?? throw new DomainException(UserErrors.InvalidRefreshToken());

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub")
            ?? throw new DomainException(UserErrors.InvalidRefreshToken());

        var userId = Guid.Parse(userIdClaim.Value);
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new DomainException(UserErrors.NotFound(userId));

        // Verify refresh token hash matches
        var refreshHash = Convert.ToBase64String(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(cmd.RefreshToken)));
        user.ValidateRefreshToken(refreshHash);

        // Rotate: generate new pair, invalidate old
        var tokens = await _jwt.GenerateTokenPairAsync(user, ct);
        var newRefreshHash = Convert.ToBase64String(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(tokens.RefreshToken)));
        user.SetRefreshToken(newRefreshHash, TimeSpan.FromDays(7));

        return new LoginResponse(
            tokens.AccessToken, tokens.RefreshToken,
            tokens.ExpiresAt, user.Id, user.Username);
    }
}