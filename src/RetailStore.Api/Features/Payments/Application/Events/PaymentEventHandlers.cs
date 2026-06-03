using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Payments.Application;
using RetailStore.Api.Features.Payments.Application.Commands;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Api.Features.Shipping.Application.Commands;

namespace RetailStore.Api.Features.Payments.Application.Events;

public sealed class PaymentCapturedHandler(
    ISender sender, ILogger<PaymentCapturedHandler> log)
    : INotificationHandler<PaymentCapturedEvent>
{
    public async Task Handle(PaymentCapturedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Payment {PaymentId} captured: {Amount} {Currency} for order {OrderId} — creating shipment",
            e.PaymentId, e.Amount, e.Currency, e.OrderId);

        try
        {
            await sender.Send(new CreateShipmentCommand(e.OrderId), ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex,
                "Failed to auto-create shipment for order {OrderId} after payment capture",
                e.OrderId);
        }
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
        return Task.CompletedTask;
    }
}

// ─── Cross-module: Cancel pending payment when order is cancelled ──
public sealed class CancelPaymentOnOrderCancelled(
    IPaymentRepository payments,
    ISender sender,
    ILogger<CancelPaymentOnOrderCancelled> log)
    : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent e, CancellationToken ct)
    {
        var payment = await payments.GetByOrderIdAsync(e.OrderId, ct);
        if (payment is null)
            return;

        if (payment.Status is PaymentStatus.Pending or PaymentStatus.Authorized)
        {
            log.LogInformation(
                "Order {OrderId} cancelled — cancelling payment {PaymentId}",
                e.OrderId, payment.Id);

            await sender.Send(
                new CancelPaymentCommand(payment.Id, $"Order cancelled: {e.Reason}"), ct);
        }
        else
        {
            log.LogInformation(
                "Order {OrderId} cancelled — payment {PaymentId} in status {Status}, no action taken",
                e.OrderId, payment.Id, payment.Status);
        }
    }
}
