using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Providers.Domain;
 
public static class ProviderErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound(Guid id) => new(
        "PROVIDER_NOT_FOUND", $"Provider '{id}' does not exist.",
        DomainErrorType.NotFound);
 
    public static DomainError NotFoundByEmail(string email) => new(
        "PROVIDER_NOT_FOUND", $"No provider with email '{email}'.",
        DomainErrorType.NotFound);
 
    // ─── Conflict ──────────────────────────────────────────
    public static DomainError DuplicateEmail(string email) => new(
        "PROVIDER_DUPLICATE_EMAIL", $"A provider with email '{email}' already exists.",
        DomainErrorType.Conflict);
 
    public static DomainError AlreadyDeactivated() => new(
        "PROVIDER_ALREADY_DEACTIVATED", "Provider is already deactivated.",
        DomainErrorType.Conflict);
 
    public static DomainError AlreadyActive() => new(
        "PROVIDER_ALREADY_ACTIVE", "Provider is already active.",
        DomainErrorType.Conflict);
 
    public static DomainError ProductAlreadyAssociated(Guid productId) => new(
        "PROVIDER_PRODUCT_ALREADY_ASSOCIATED",
        $"Product '{productId}' is already associated with this provider.",
        DomainErrorType.Conflict);
 
    public static DomainError ProductNotAssociated(Guid productId) => new(
        "PROVIDER_PRODUCT_NOT_ASSOCIATED",
        $"Product '{productId}' is not associated with this provider.",
        DomainErrorType.NotFound);
 
    // ─── Validation ────────────────────────────────────────
    public static DomainError InvalidCompanyName() => new(
        "PROVIDER_INVALID_COMPANY_NAME", "Company name must be at least 2 characters.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidContactName() => new(
        "PROVIDER_INVALID_CONTACT_NAME", "Contact name must be at least 2 characters.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidEmail(string email) => new(
        "PROVIDER_INVALID_EMAIL", $"'{email}' is not a valid email address.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidPhone(string phone) => new(
        "PROVIDER_INVALID_PHONE", $"'{phone}' is not a valid phone number.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidProductId() => new(
        "PROVIDER_INVALID_PRODUCT_ID", "A valid product ID is required.",
        DomainErrorType.Validation);
 
    // ─── Business Rules ────────────────────────────────────
    public static DomainError InactiveProviderCannotSupply() => new(
        "PROVIDER_INACTIVE_CANNOT_SUPPLY",
        "An inactive provider cannot be associated with products.",
        DomainErrorType.BusinessRule);
}