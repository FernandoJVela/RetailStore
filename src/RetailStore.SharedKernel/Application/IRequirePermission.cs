namespace RetailStore.SharedKernel.Application;

/// <summary>
/// Marker interface. Commands implementing this are checked by
/// AuthorizationBehavior against the permission cache.
/// </summary>
public interface IRequirePermission
{
    /// <summary>
    /// The permission required, e.g., "products:write".
    /// </summary>
    string RequiredPermission { get; }
}