using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;

namespace RetailStore.Api.Features.Orders.Domain;

/// <summary>
/// Static error catalog for the Orders module.
/// Every possible error is defined here. Acts as living documentation.
/// </summary>
public static class OrderErrors
{
    public static DomainError OrderNotFound(Guid id) => new(
        "ORDER_NOT_FOUND", $"Order '{id}' not found.", DomainErrorType.NotFound);
    public static DomainError OrderNotFound() => new(
        "ORDER_LIST_EMPTY", "No orders found.", DomainErrorType.NotFound);
    public static DomainError CustomerNotFound(Guid id) => new(
        "ORDER_CUSTOMER_NOT_FOUND", $"Customer '{id}' not found.", DomainErrorType.NotFound);
    public static DomainError InvalidOrderDate() => new(
        "ORDER_INVALID_DATE", "Order date cannot be in the future.", DomainErrorType.BusinessRule);
    public static DomainError CannotCompleteEmptyOrder() => new(
        "ORDER_CANNOT_COMPLETE_EMPTY", "Cannot complete an order without items.", DomainErrorType.BusinessRule);
    public static DomainError OrderAlreadyCompleted() => new(
        "ORDER_ALREADY_COMPLETED", "Order is already completed or cancelled.", DomainErrorType.Conflict);
    public static DomainError InvalidOrderStatusTransition(OrderStatus from, OrderStatus to) => new(
        "ORDER_INVALID_STATUS_TRANSITION", $"Cannot transition from '{from}' to '{to}'.", DomainErrorType.BusinessRule);
    public static DomainError InvalidOrderStatusForModification(OrderStatus status) => new(
        "ORDER_INVALID_STATUS_FOR_MODIFICATION", $"Cannot modify order in '{status}' status.", DomainErrorType.BusinessRule);
    public static DomainError InvalidOrderItem(Guid productId) => new(
        "ORDER_INVALID_ITEM", $"No item with product '{productId}' in this order.", DomainErrorType.NotFound);
    public static DomainError InvalidOrderItemPrice(Guid productId) => new(
        "ORDER_INVALID_ITEM_PRICE", $"Price for product '{productId}' must be positive.", DomainErrorType.BusinessRule);
}