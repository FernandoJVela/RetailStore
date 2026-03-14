using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Users.Infrastructure;

public interface IJwtTokenService
{
    Task<TokenPair> GenerateTokenPairAsync(User user, CancellationToken ct);
    ClaimsPrincipal? ValidateExpiredToken(string token);
}

public record TokenPair(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly IPermissionService _permissions;

    public JwtTokenService(IConfiguration config, IPermissionService permissions)
    { _config = config; _permissions = permissions; }

    public async Task<TokenPair> GenerateTokenPairAsync(User user, CancellationToken ct)
    {
        var permissions = await _permissions.GetPermissionsAsync(user.Id, ct);

        // ─── Claims Projection ─────────────────────────────
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Project each permission as a claim
        foreach (var perm in permissions)
            claims.Add(new Claim("permission", perm));

        // ─── Access Token ─────────────────────────────────
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var expiry = DateTime.UtcNow.AddMinutes(
            int.Parse(_config["Jwt:ExpiryMinutes"] ?? "30"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // ─── Refresh Token ────────────────────────────────
        var refreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        return new TokenPair(accessToken, refreshToken, expiry);
    }

    public ClaimsPrincipal? ValidateExpiredToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!)),
            ValidateLifetime = false  // Allow expired tokens for refresh
        };
        try
        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }
        catch { return null; }
    }
}