using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Domain;

public sealed class Role : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }  // System roles can't be deleted

    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public static Role Create(string name, string? description = null, bool isSystem = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description,
            IsSystem = isSystem
        };
    }

    public void AddPermission(Permission permission)
    {
        if (_permissions.Any(p => p.FullName == permission.FullName)) return;
        _permissions.Add(permission);
        Touch();
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.RemoveAll(p => p.FullName == permission.FullName);
        Touch();
    }

    public void UpdatePermissions(List<Permission> newPermissions)
    {
        var added = newPermissions.Where(n => !_permissions.Any(e => e.FullName == n.FullName)).ToList();
        var removed = _permissions.Where(e => !newPermissions.Any(n => n.FullName == e.FullName)).ToList();

        _permissions.Clear();
        _permissions.AddRange(newPermissions);
        Touch(); IncrementVersion();

        if (added.Any() || removed.Any())
            Raise(new RolePermissionsChangedEvent(
                Id, Name, added.Select(p => p.FullName).ToList(),
                removed.Select(p => p.FullName).ToList()));
    }
}