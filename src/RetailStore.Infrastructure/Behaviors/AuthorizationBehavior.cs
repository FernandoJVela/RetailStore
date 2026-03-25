using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Infrastructure.Behaviors;

public sealed class AuthorizationBehavior<TReq, TRes>
    : IPipelineBehavior<TReq, TRes> where TReq : notnull
{
    private readonly IHttpContextAccessor _http;
    private readonly IPermissionService _permissions;

    public AuthorizationBehavior(
        IHttpContextAccessor http,
        IPermissionService permissions)
    { _http = http; _permissions = permissions; }

    public async Task<TRes> Handle(
        TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        if (request is not IRequirePermission permReq)
            return await next();

        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            throw new DomainException(new DomainError(
                "UNAUTHORIZED", "Authentication required.",
                DomainErrorType.Unauthorized));

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub");
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new DomainException(new DomainError(
                "UNAUTHORIZED", "Invalid user identity.",
                DomainErrorType.Unauthorized));

        // ─── Fast path: check JWT claims first ────────────
        var jwtPermissions = user.FindAll("permission")
            .Select(c => c.Value).ToHashSet();

        if (jwtPermissions.Count > 0 && CheckPermission(
            jwtPermissions, permReq.RequiredPermission))
            return await next();

        // ─── Cache path: check live permission cache ──────
        var hasPermission = await _permissions.HasPermissionAsync(
            userId, permReq.RequiredPermission, ct);

        if (!hasPermission)
            throw new DomainException(new DomainError(
                "FORBIDDEN",
                $"Permission '{permReq.RequiredPermission}' is required.",
                DomainErrorType.Forbidden));

        return await next();
    }

    private static bool CheckPermission(
        HashSet<string> userPermissions, string required)
    {
        var req = Permission.Parse(required);
        return userPermissions.Any(p =>
        {
            var existing = Permission.Parse(p);
            return existing.Satisfies(req);
        });
    }
}