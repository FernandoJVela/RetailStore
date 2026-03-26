using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Payments.Domain;
 
namespace RetailStore.Api.Features.Payments.Application.Events;
 
public sealed class PaymentCapturedHandler(ILogger<PaymentCapturedHandler> log)
    : INotificationHandler<PaymentCapturedEvent>
{
    public Task Handle(PaymentCapturedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Payment {PaymentId} captured: {Amount} {Currency} for order {OrderId}",
            e.PaymentId, e.Amount, e.Currency, e.OrderId);
        // TODO: Update order status
        // TODO: Trigger shipment creation
        // TODO: Send payment confirmation notification
        return Task.CompletedTask;
    }
}
 
public sealed class PaymentFailedHandler(ILogger<PaymentFailedHandler> log)
    : INotificationHandler<PaymentFailedEvent>
{
    public Task Handle(PaymentFailedEvent e, CancellationToken ct)
    {
        log.LogError(
            "Payment {PaymentId} FAILED for order {OrderId}: {Reason}",
            e.PaymentId, e.OrderId, e.Reason);
        // TODO: Notify customer
        // TODO: Retry with different method?
        return Task.CompletedTask;
    }
}
 
public sealed class RefundCompletedHandler(ILogger<RefundCompletedHandler> log)
    : INotificationHandler<RefundCompletedEvent>
{
    public Task Handle(RefundCompletedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Refund {RefundId} completed: {Amount} {Currency} for payment {PaymentId}",
            e.RefundId, e.Amount, e.Currency, e.PaymentId);
        // TODO: Notify customer of refund
        return Task.CompletedTask;
    }
}
 
public sealed class PaymentFullyRefundedHandler(ILogger<PaymentFullyRefundedHandler> log)
    : INotificationHandler<PaymentFullyRefundedEvent>
{
    public Task Handle(PaymentFullyRefundedEvent e, CancellationToken ct)
    {
        log.LogWarning(
            "Payment {PaymentId} fully refunded: {Amount} {Currency} for order {OrderId}",
            e.PaymentId, e.OriginalAmount, e.Currency, e.OrderId);
        // TODO: Release inventory reservations
        // TODO: Cancel shipment if not shipped
        return Task.CompletedTask;
    }
}
 
// ─── Cross-module: React to order cancellation ──────────────
public sealed class CancelPaymentOnOrderCancelled(ILogger<CancelPaymentOnOrderCancelled> log)
    : INotificationHandler<OrderCancelledEvent>
{
    public Task Handle(OrderCancelledEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Order {OrderId} cancelled — payment should be cancelled/refunded",
            e.OrderId);
        // TODO: Load payment by OrderId, cancel or refund based on status
        return Task.CompletedTask;
    }
}