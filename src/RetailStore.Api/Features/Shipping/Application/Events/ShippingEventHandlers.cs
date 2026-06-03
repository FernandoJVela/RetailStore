using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Orders.Application.Commands;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Shipping.Application.Commands;
using RetailStore.Api.Features.Shipping.Domain;

namespace RetailStore.Api.Features.Shipping.Application.Events;

public sealed class ShipmentShippedHandler(
    ISender sender, ILogger<ShipmentShippedHandler> log)
    : INotificationHandler<ShipmentShippedEvent>
{
    public async Task Handle(ShipmentShippedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Shipment {ShipmentId} shipped via {Carrier} (tracking: {Tracking}) — marking order {OrderId} as shipped",
            e.ShipmentId, e.Carrier, e.TrackingNumber, e.OrderId);

        await sender.Send(new MarkOrderShippedCommand(e.OrderId), ct);
    }
}

public sealed class ShipmentDeliveredHandler(
    ISender sender, ILogger<ShipmentDeliveredHandler> log)
    : INotificationHandler<ShipmentDeliveredEvent>
{
    public async Task Handle(ShipmentDeliveredEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Shipment {ShipmentId} delivered — marking order {OrderId} as delivered",
            e.ShipmentId, e.OrderId);

        await sender.Send(new MarkOrderDeliveredCommand(e.OrderId), ct);
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
        return Task.CompletedTask;
    }
}

// ─── Cross-module: Auto-create shipment after payment captured ──
public sealed class CreateShipmentOnOrderConfirmedHandler(ILogger<CreateShipmentOnOrderConfirmedHandler> log)
    : INotificationHandler<OrderConfirmedEvent>
{
    public Task Handle(OrderConfirmedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Order {OrderId} confirmed — shipment will be created after payment is captured",
            e.OrderId);
        return Task.CompletedTask;
    }
}
