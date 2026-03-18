using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;

namespace RetailStore.Api.Features.Orders.Domain;

/// <summary>
/// Static error catalog for the Orders module.
/// Every possible error is defined here. Acts as living documentation.
/// </summary>
public static class OrderErrors
{
    public static DomainError CannotCompleteEmptyOrder() => new(
        "ORDER_CANNOT_COMPLETE_EMPTY",
        "Cannot complete an order without items.",
        DomainErrorType.BusinessRule);

    public static DomainError OrderAlreadyCompleted() => new(
        "ORDER_ALREADY_COMPLETED",
        "Order is already completed.",
        DomainErrorType.BusinessRule);

    public static DomainError CannotModifyCompletedOrder() => new(
        "ORDER_MODIFY_COMPLETED",
        "Cannot modify a completed order.",
        DomainErrorType.BusinessRule);

    public static DomainError CustomerNotFound(Guid customerId) => new(
        "ORDER_CUSTOMER_NOT_FOUND",
        $"Customer with ID '{customerId}' does not exist.",
        DomainErrorType.NotFound);

    public static DomainError OrderNotFound(Guid orderId) => new(
        "ORDER_NOT_FOUND",
        $"Order with ID '{orderId}' does not exist.",
        DomainErrorType.NotFound);

    public static DomainError InvalidOrderStatusTransition(OrderStatus from, OrderStatus to) => new(
        "ORDER_INVALID_STATUS_TRANSITION",
        $"Cannot transition order status from '{from}' to '{to}'.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidOrderDate() => new(
        "ORDER_INVALID_DATE",
        "Order date cannot be in the future.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidOrderStatusForModification(OrderStatus status) => new(
        "ORDER_INVALID_STATUS_FOR_MODIFICATION",
        $"Cannot modify order in '{status}' status.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidEmptyOrder() => new(
        "ORDER_EMPTY",
        "Order must have at least one item.",
        DomainErrorType.BusinessRule);

    public static DomainError InvalidOrderItem(Guid productId) => new(
        "ORDER_INVALID_ITEM",
        $"Order does not contain an item with Product ID '{productId}'.",
        DomainErrorType.BusinessRule);
}