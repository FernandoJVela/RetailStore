using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Domain;

public sealed class User : AggregateRoot
{
    public string Username { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    private readonly List<Guid> _roleIds = new();
    public IReadOnlyCollection<Guid> RoleIds => _roleIds.AsReadOnly();

    private User() { }

    // ─── Factory ──────────────────────────────────────────
    public static User Register(
        string username, string email, string plainPassword)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim(),
            Email = new Email(email),
            PasswordHash = PasswordHash.Create(plainPassword)
        };
        user.Raise(new UserRegisteredEvent(user.Id, username, email));
        return user;
    }

    // ─── Authentication ───────────────────────────────────
    public void ValidateCredentials(string plainPassword)
    {
        if (!IsActive)
            throw new DomainException(UserErrors.AccountDeactivated());
        if (!PasswordHash.Verify(plainPassword))
            throw new DomainException(UserErrors.InvalidCredentials());

        LastLoginAt = DateTime.UtcNow;
        Touch();
        Raise(new UserLoggedInEvent(Id, LastLoginAt.Value));
    }

    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!PasswordHash.Verify(currentPassword))
            throw new DomainException(UserErrors.InvalidCredentials());
        PasswordHash = PasswordHash.Create(newPassword);
        InvalidateRefreshToken();
        Touch();
        Raise(new UserPasswordChangedEvent(Id));
    }

    // ─── Refresh Token ────────────────────────────────────
    public void SetRefreshToken(string tokenHash, TimeSpan lifetime)
    {
        var oldHash = RefreshTokenHash;
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = DateTime.UtcNow.Add(lifetime);
        Touch();
        if (oldHash is not null)
            Raise(new RefreshTokenRotatedEvent(Id, oldHash));
    }

    public void ValidateRefreshToken(string tokenHash)
    {
        if (RefreshTokenHash != tokenHash || RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new DomainException(UserErrors.InvalidRefreshToken());
    }

    public void InvalidateRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
    }

    // ─── Role Management ─────────────────────────────────
    public void AssignRole(Guid roleId, string roleName)
    {
        if (_roleIds.Contains(roleId))
            throw new DomainException(UserErrors.RoleAlreadyAssigned(roleName));
        _roleIds.Add(roleId);
        Touch();
        Raise(new UserRoleAssignedEvent(Id, roleName));
    }

    public void RevokeRole(Guid roleId, string roleName)
    {
        _roleIds.Remove(roleId);
        Touch();
        Raise(new UserRoleRevokedEvent(Id, roleName));
    }

    // ─── Lifecycle ────────────────────────────────────────
    public void Deactivate()
    {
        if (!IsActive) throw new DomainException(UserErrors.AlreadyDeactivated());
        IsActive = false;
        InvalidateRefreshToken();
        Touch();
        Raise(new UserDeactivatedEvent(Id));
    }

    public void Reactivate()
    {
        IsActive = true;
        Touch();
        Raise(new UserReactivatedEvent(Id));
    }
}