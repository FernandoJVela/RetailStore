using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RetailStore.Tests.TestHelpers;

/// <summary>
/// Generates signed JWT tokens for integration tests.
/// Tokens include the "permission" claim that the AuthorizationBehavior reads directly
/// from the JWT (fast path), so no DB role lookup is needed during tests.
/// </summary>
public static class TestJwtHelper
{
    /// <summary>Generates an admin token that satisfies every permission check ("*:*").</summary>
    public static string AdminToken(
        string secret, string issuer, string audience, Guid? userId = null)
        => GenerateToken(["*:*"], secret, issuer, audience, userId);

    /// <summary>Generates a token with exactly the listed permissions.</summary>
    public static string GenerateToken(
        string[] permissions,
        string secret,
        string issuer,
        string audience,
        Guid? userId = null)
    {
        // "sub" maps to ClaimTypes.NameIdentifier via ASP.NET Core JWT Bearer middleware,
        // which is what AuthorizationBehavior reads (FindFirst(ClaimTypes.NameIdentifier) ?? FindFirst("sub")).
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var perm in permissions)
            claims.Add(new Claim("permission", perm));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
