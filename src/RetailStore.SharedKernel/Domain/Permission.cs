namespace RetailStore.SharedKernel.Domain;

/// <summary>
/// Permission follows the format "resource:action".
/// Examples: products:write, orders:read, inventory:adjust, users:manage
/// Wildcard: products:* means all actions on products.
/// </summary>
public sealed class Permission : ValueObject
{
    public string Resource { get; }  // e.g., "products"
    public string Action { get; }    // e.g., "write" or "*"
    public string FullName => $"{Resource}:{Action}";

    public Permission(string resource, string action)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new DomainException(new DomainError(
                "USER_INVALID_PERMISSION", 
                $"'{resource}:{action}' is not a valid permission format (resource:action). Resource cannot be empty.",
                DomainErrorType.Validation)
            );
        Resource = resource.ToLowerInvariant().Trim();
        Action = action.ToLowerInvariant().Trim();
    }

    public static Permission Parse(string fullName)
    {
        var parts = fullName.Split(':');
        if (parts.Length != 2)
            throw new DomainException(new DomainError(
                "USER_INVALID_PERMISSION", 
                $"'{fullName}' is not a valid permission format (resource:action).",
                DomainErrorType.Validation)
            );
        return new Permission(parts[0], parts[1]);
    }

    /// <summary>
    /// Checks if this permission satisfies a required permission.
    /// A wildcard (products:*) satisfies any action (products:write).
    /// </summary>
    public bool Satisfies(Permission required)
    {
        if (Resource != required.Resource) return false;
        if (Action == "*") return true;
        return Action == required.Action;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    { yield return Resource; yield return Action; }

    public override string ToString() => FullName;
}
