using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Payments.Domain;
 
public static class PaymentErrors
{
    public static DomainError NotFound(Guid id) => new(
        "PAYMENT_NOT_FOUND", $"Payment '{id}' not found.", DomainErrorType.NotFound);
    public static DomainError NotFoundByOrder(Guid orderId) => new(
        "PAYMENT_NOT_FOUND_BY_ORDER", $"No payment for order '{orderId}'.", DomainErrorType.NotFound);
    public static DomainError AlreadyExists(Guid orderId) => new(
        "PAYMENT_ALREADY_EXISTS", $"A payment for order '{orderId}' already exists.", DomainErrorType.Conflict);
    public static DomainError InvalidAmount() => new(
        "PAYMENT_INVALID_AMOUNT", "Amount must be greater than zero.", DomainErrorType.Validation);
    public static DomainError InvalidCurrency() => new(
        "PAYMENT_INVALID_CURRENCY", "Currency must be 3-letter ISO code.", DomainErrorType.Validation);
    public static DomainError InvalidOrderId() => new(
        "PAYMENT_INVALID_ORDER_ID", "A valid order ID is required.", DomainErrorType.Validation);
    public static DomainError InvalidCustomerId() => new(
        "PAYMENT_INVALID_CUSTOMER_ID", "A valid customer ID is required.", DomainErrorType.Validation);
    public static DomainError InvalidGateway() => new(
        "PAYMENT_INVALID_GATEWAY", "Gateway name is required.", DomainErrorType.Validation);
    public static DomainError InvalidStatusTransition(PaymentStatus from, PaymentStatus to) => new(
        "PAYMENT_INVALID_STATUS", $"Cannot transition from '{from}' to '{to}'.", DomainErrorType.BusinessRule);
    public static DomainError CannotRefundUncaptured() => new(
        "PAYMENT_CANNOT_REFUND_UNCAPTURED", "Only captured payments can be refunded.", DomainErrorType.BusinessRule);
    public static DomainError RefundExceedsPayment(decimal requested, decimal available) => new(
        "PAYMENT_REFUND_EXCEEDS", $"Refund {requested:F2} exceeds available {available:F2}.", DomainErrorType.BusinessRule);
    public static DomainError RefundNotFound(Guid refundId) => new(
        "PAYMENT_REFUND_NOT_FOUND", $"Refund '{refundId}' not found.", DomainErrorType.NotFound);
    public static DomainError InvalidRefundAmount() => new(
        "PAYMENT_INVALID_REFUND_AMOUNT", "Refund amount must be positive.", DomainErrorType.Validation);
    public static DomainError RefundReasonRequired() => new(
        "PAYMENT_REFUND_REASON_REQUIRED", "Refund reason is required.", DomainErrorType.Validation);
    public static DomainError OrderNotConfirmed() => new(
        "PAYMENT_ORDER_NOT_CONFIRMED", "Payment can only be created for confirmed orders.", DomainErrorType.BusinessRule);
}