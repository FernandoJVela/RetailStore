using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Domain;

/// <summary>
/// Static error catalog for the Order Items module.
/// Every possible error is defined here. Acts as living documentation.
/// </summary>
public static class OrderItemErrors
{
    public static DomainError InvalidQuantity() => new(
        "ORDER_ITEM_INVALID_QUANTITY", "Quantity must be greater than zero.", DomainErrorType.BusinessRule);
    public static DomainError InvalidUnitPrice() => new(
        "ORDER_ITEM_INVALID_UNIT_PRICE", "Unit price must be greater than zero.", DomainErrorType.BusinessRule);
    public static DomainError OrderItemNotFound(Guid id) => new(
        "ORDER_ITEM_NOT_FOUND", $"Order item '{id}' not found.", DomainErrorType.NotFound);
}