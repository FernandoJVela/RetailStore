using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Infrastructure;

public sealed class CachedPermissionService : IPermissionService
{
    private readonly RetailStoreDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedPermissionService> _log;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public CachedPermissionService(
        RetailStoreDbContext db, IMemoryCache cache,
        ILogger<CachedPermissionService> log)
    { _db = db; _cache = cache; _log = log; }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(
        Guid userId, CancellationToken ct)
    {
        var cacheKey = $"user:{userId}:permissions";

        if (_cache.TryGetValue(cacheKey, out IReadOnlySet<string>? cached))
        {
            _log.LogDebug("Permission cache HIT for user {UserId}", userId);
            return cached!;
        }

        _log.LogDebug("Permission cache MISS for user {UserId}", userId);

        // Load user's role IDs
        var user = await _db.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return new HashSet<string>();

        // Load all roles with their permissions
        var roleIds = user.RoleIds.ToList();
        var roles = await _db.Set<Role>()
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(ct);

        // Flatten: union of all permissions across all roles
        var permissions = roles
            .SelectMany(r => r.Permissions)
            .Select(p => p.FullName)
            .ToHashSet();

        var result = (IReadOnlySet<string>)permissions;
        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = permissions.Count  // For cache size limiting
        });

        _log.LogInformation(
            "Cached {Count} permissions for user {UserId}",
            permissions.Count, userId);

        return result;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, string permission, CancellationToken ct)
    {
        var permissions = await GetPermissionsAsync(userId, ct);
        var required = Permission.Parse(permission);

        // Check exact match or wildcard
        return permissions.Any(p =>
        {
            var existing = Permission.Parse(p);
            return existing.Satisfies(required);
        });
    }

    public void InvalidateCache(Guid userId)
    {
        _cache.Remove($"user:{userId}:permissions");
        _log.LogInformation("Invalidated permission cache for user {UserId}", userId);
    }

    public void InvalidateCacheForRole(Guid roleId)
    {
        // When a role changes, we must invalidate ALL users with that role.
        // In production, use IDistributedCache with tag-based invalidation
        // or maintain a role->users reverse index.
        // For IMemoryCache, we clear the entire permission cache space.
        if (_cache is MemoryCache mc)
            mc.Compact(1.0); // Nuclear option for IMemoryCache
        _log.LogWarning(
            "Invalidated ALL permission caches due to role {RoleId} change", roleId);
    }
}