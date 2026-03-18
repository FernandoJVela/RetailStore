using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Domain;

/// <summary>
/// Static error catalog for the Order Items module.
/// Every possible error is defined here. Acts as living documentation.
/// </summary>
public static class OrderItemErrors
{
    public static DomainError InvalidQuantity() => new(
        "ORDER_ITEM_INVALID_QUANTITY",
        "Order item quantity must be greater than zero.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidUnitPrice() => new(
        "ORDER_ITEM_INVALID_UNIT_PRICE",
        "Order item unit price must be greater than zero.",
        DomainErrorType.BusinessRule);

    public static DomainError CannotModifyCompletedOrder() => new(
        "ORDER_ITEM_MODIFY_COMPLETED_ORDER",
        "Cannot modify items of a completed order.",
        DomainErrorType.BusinessRule);

    public static DomainError OrderItemNotFound(Guid productId) => new(
        "ORDER_ITEM_NOT_FOUND",
        $"Order item with product ID '{productId}' does not exist in the order.",
        DomainErrorType.NotFound);

    public static DomainError ProductNotFound(Guid productId) => new(
        "ORDER_ITEM_PRODUCT_NOT_FOUND",
        $"Product with ID '{productId}' does not exist.",
        DomainErrorType.NotFound);

    public static DomainError InsufficientStock(Guid productId) => new(
        "ORDER_ITEM_INSUFFICIENT_STOCK",
        $"Insufficient stock for product with ID '{productId}'.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidProductName() => new(
        "ORDER_ITEM_INVALID_PRODUCT_NAME",
        "Product name is required for order items.",
        DomainErrorType.BusinessRule);
}