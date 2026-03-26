using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Payments.Domain;
 
public sealed record PaymentCreatedEvent(
    Guid PaymentId, Guid OrderId, Guid CustomerId,
    decimal Amount, string Currency, string Method) : DomainEvent
{ public override string EventType => "PaymentCreated"; }
 
public sealed record PaymentAuthorizedEvent(
    Guid PaymentId, Guid OrderId,
    decimal Amount, string Currency, string? GatewayTransactionId) : DomainEvent
{ public override string EventType => "PaymentAuthorized"; }
 
public sealed record PaymentCapturedEvent(
    Guid PaymentId, Guid OrderId, Guid CustomerId,
    decimal Amount, string Currency) : DomainEvent
{ public override string EventType => "PaymentCaptured"; }
 
public sealed record PaymentFailedEvent(
    Guid PaymentId, Guid OrderId, string Reason) : DomainEvent
{ public override string EventType => "PaymentFailed"; }
 
public sealed record PaymentCancelledEvent(
    Guid PaymentId, Guid OrderId, string? Reason) : DomainEvent
{ public override string EventType => "PaymentCancelled"; }
 
public sealed record PaymentExpiredEvent(
    Guid PaymentId, Guid OrderId) : DomainEvent
{ public override string EventType => "PaymentExpired"; }
 
public sealed record RefundRequestedEvent(
    Guid PaymentId, Guid RefundId, Guid OrderId,
    decimal Amount, string Currency, string Reason) : DomainEvent
{ public override string EventType => "RefundRequested"; }
 
public sealed record RefundCompletedEvent(
    Guid PaymentId, Guid RefundId,
    decimal Amount, string Currency) : DomainEvent
{ public override string EventType => "RefundCompleted"; }
 
public sealed record PaymentFullyRefundedEvent(
    Guid PaymentId, Guid OrderId,
    decimal OriginalAmount, string Currency) : DomainEvent
{ public override string EventType => "PaymentFullyRefunded"; }
 
public sealed record RefundFailedEvent(
    Guid PaymentId, Guid RefundId, string Reason) : DomainEvent
{ public override string EventType => "RefundFailed"; }