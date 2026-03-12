using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Domain;

/// <summary>
/// Static error catalog for the Products module.
/// Every possible error is defined here. Acts as living documentation.
/// </summary>
public static class ProductErrors
{
    public static DomainError NotFound(Guid id) => new(
        "PRODUCT_NOT_FOUND",
        $"Product with ID '{id}' does not exist.",
        DomainErrorType.NotFound);
        
    public static DomainError Empty() => new(
        "PRODUCTS_NOT_FOUND",
        $"Product list is empty.",
        DomainErrorType.NotFound);

    public static DomainError DuplicateSku(string sku) => new(
        "PRODUCT_DUPLICATE_SKU",
        $"A product with SKU '{sku}' already exists.",
        DomainErrorType.Conflict);

    public static DomainError InvalidPrice() => new(
        "PRODUCT_INVALID_PRICE",
        "Product price must be greater than zero.",
        DomainErrorType.BusinessRule);

    public static DomainError AlreadyDeactivated(Guid id) => new(
        "PRODUCT_ALREADY_DEACTIVATED",
        $"Product '{id}' is already deactivated.",
        DomainErrorType.Conflict);
}
