namespace RetailStore.SharedKernel.Application;

public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default);
    void InvalidateCache(Guid userId);
    void InvalidateCacheForRole(Guid roleId);
}