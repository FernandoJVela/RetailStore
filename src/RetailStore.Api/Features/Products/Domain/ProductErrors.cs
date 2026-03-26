using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Products.Domain;
 
public static class ProductErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound(Guid id) => new(
        "PRODUCT_NOT_FOUND", $"Product '{id}' does not exist.", DomainErrorType.NotFound);
 
    public static DomainError Empty() => new(
        "PRODUCTS_NOT_FOUND", "No products match the search criteria.", DomainErrorType.NotFound);
 
    // ─── Conflict ──────────────────────────────────────────
    public static DomainError DuplicateSku(string sku) => new(
        "PRODUCT_DUPLICATE_SKU", $"A product with SKU '{sku}' already exists.", DomainErrorType.Conflict);
 
    public static DomainError AlreadyDeactivated() => new(
        "PRODUCT_ALREADY_DEACTIVATED", "Product is already deactivated.", DomainErrorType.Conflict);
 
    public static DomainError AlreadyActive() => new(
        "PRODUCT_ALREADY_ACTIVE", "Product is already active.", DomainErrorType.Conflict);
 
    // ─── Validation ────────────────────────────────────────
    public static DomainError InvalidPrice() => new(
        "PRODUCT_INVALID_PRICE", "Product price must be greater than zero.", DomainErrorType.BusinessRule);
 
    public static DomainError InvalidName() => new(
        "PRODUCT_INVALID_NAME", "Product name is required.", DomainErrorType.Validation);
 
    public static DomainError InvalidSku() => new(
        "PRODUCT_INVALID_SKU", "Product SKU is required.", DomainErrorType.Validation);
 
    public static DomainError InvalidCategory() => new(
        "PRODUCT_INVALID_CATEGORY", "Product category is required.", DomainErrorType.Validation);
 
    // ─── Business Rules ────────────────────────────────────
    public static DomainError CannotOrderInactive(Guid id) => new(
        "PRODUCT_INACTIVE_CANNOT_ORDER", $"Product '{id}' is inactive and cannot be ordered.", DomainErrorType.BusinessRule);
}