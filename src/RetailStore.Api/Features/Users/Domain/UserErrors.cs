using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Domain;

public static class UserErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound() => new(
        "USERS_NOT_FOUND", $"Users not available.",
        DomainErrorType.NotFound);
    public static DomainError NotFound(Guid id) => new(
        "USER_NOT_FOUND", $"User '{id}' does not exist.",
        DomainErrorType.NotFound);

    public static DomainError NotFoundByEmail(string email) => new(
        "USER_NOT_FOUND", $"No user with email '{email}'.",
        DomainErrorType.NotFound);

    // ─── Conflict ──────────────────────────────────────────
    public static DomainError DuplicateEmail(string email) => new(
        "USER_DUPLICATE_EMAIL", $"Email '{email}' is already registered.",
        DomainErrorType.Conflict);

    public static DomainError DuplicateUsername(string username) => new(
        "USER_DUPLICATE_USERNAME", $"Username '{username}' is taken.",
        DomainErrorType.Conflict);

    public static DomainError AlreadyDeactivated() => new(
        "USER_ALREADY_DEACTIVATED", "User is already deactivated.",
        DomainErrorType.Conflict);

    public static DomainError RoleAlreadyAssigned(string role) => new(
        "USER_ROLE_ALREADY_ASSIGNED", $"Role '{role}' is already assigned.",
        DomainErrorType.Conflict);

    public static DomainError EmailAlreadyInUse(string email) => new(
        "USER_EMAIL_ALREADY_IN_USE", $"Email '{email}' is already in use.",
        DomainErrorType.Conflict);

    // ─── Business Rules ───────────────────────────────────
    public static DomainError InvalidEmail(string email) => new(
        "USER_INVALID_EMAIL", $"'{email}' is not a valid email.",
        DomainErrorType.Validation);

    public static DomainError WeakPassword() => new(
        "USER_WEAK_PASSWORD", "Password must be at least 8 characters.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidCredentials() => new(
        "USER_INVALID_CREDENTIALS", "Email or password is incorrect.",
        DomainErrorType.Unauthorized);

    public static DomainError AccountDeactivated() => new(
        "USER_ACCOUNT_DEACTIVATED", "This account has been deactivated.",
        DomainErrorType.Forbidden);

    public static DomainError InvalidPermission(string perm) => new(
        "USER_INVALID_PERMISSION", $"'{perm}' is not a valid permission format (resource:action).",
        DomainErrorType.Validation);

    public static DomainError InvalidRefreshToken() => new(
        "USER_INVALID_REFRESH_TOKEN", "Refresh token is invalid or expired.",
        DomainErrorType.Unauthorized);

    public static DomainError CannotRemoveLastAdmin() => new(
        "USER_CANNOT_REMOVE_LAST_ADMIN", "Cannot remove the last admin role.",
        DomainErrorType.BusinessRule);

    // ─── Roles ────────────────────────────────────────────
    public static DomainError RoleNotFound() => new(
        "ROLES_NOT_FOUND", $"None role has been defined.",
        DomainErrorType.NotFound);
    public static DomainError RoleNotFound(Guid roleId) => new(
        "ROLE_NOT_FOUND", $"Role '{roleId}' does not exist.",
        DomainErrorType.NotFound);
}