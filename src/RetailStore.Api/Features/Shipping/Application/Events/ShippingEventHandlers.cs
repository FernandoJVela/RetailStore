using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Shipping.Domain;
 
namespace RetailStore.Api.Features.Shipping.Application.Events;
 
public sealed class ShipmentShippedHandler(ILogger<ShipmentShippedHandler> log)
    : INotificationHandler<ShipmentShippedEvent>
{
    public Task Handle(ShipmentShippedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Shipment {ShipmentId} shipped via {Carrier} (tracking: {Tracking})",
            e.ShipmentId, e.Carrier, e.TrackingNumber);
        // TODO: Send shipping notification email to customer
        // TODO: Update order status to Shipped
        return Task.CompletedTask;
    }
}
 
public sealed class ShipmentDeliveredHandler(ILogger<ShipmentDeliveredHandler> log)
    : INotificationHandler<ShipmentDeliveredEvent>
{
    public Task Handle(ShipmentDeliveredEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Shipment {ShipmentId} delivered to customer {CustomerId} at {DeliveredAt}",
            e.ShipmentId, e.CustomerId, e.DeliveredAt);
        // TODO: Send delivery confirmation email
        // TODO: Trigger order completion
        // TODO: Fulfill inventory reservations
        return Task.CompletedTask;
    }
}
 
public sealed class ShipmentFailedHandler(ILogger<ShipmentFailedHandler> log)
    : INotificationHandler<ShipmentFailedEvent>
{
    public Task Handle(ShipmentFailedEvent e, CancellationToken ct)
    {
        log.LogError(
            "Shipment {ShipmentId} FAILED for order {OrderId}: {Reason}",
            e.ShipmentId, e.OrderId, e.Reason);
        // TODO: Alert operations team
        // TODO: Schedule redelivery attempt
        return Task.CompletedTask;
    }
}
 
public sealed class ShipmentReturnedHandler(ILogger<ShipmentReturnedHandler> log)
    : INotificationHandler<ShipmentReturnedEvent>
{
    public Task Handle(ShipmentReturnedEvent e, CancellationToken ct)
    {
        log.LogWarning(
            "Shipment {ShipmentId} returned for order {OrderId}: {Reason}",
            e.ShipmentId, e.OrderId, e.Reason);
        // TODO: Release inventory reservation
        // TODO: Process refund
        return Task.CompletedTask;
    }
}
 
// ─── Cross-module: Auto-create shipment when order confirmed ──
public sealed class CreateShipmentOnOrderConfirmedHandler(
    ILogger<CreateShipmentOnOrderConfirmedHandler> log)
    : INotificationHandler<OrderConfirmedEvent>
{
    public Task Handle(OrderConfirmedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Order {OrderId} confirmed (total: {Total}) — shipment should be created",
            e.OrderId, e.TotalAmount);
        // TODO: Auto-create shipment via ISender.Send(new CreateShipmentCommand(e.OrderId))
        return Task.CompletedTask;
    }
}