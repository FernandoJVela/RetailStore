using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Orders.Domain;
 
namespace RetailStore.Api.Features.Orders.Application.Events;
 
public sealed class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> log)
    : INotificationHandler<OrderCreatedEvent>
{
    public Task Handle(OrderCreatedEvent e, CancellationToken ct)
    {
        log.LogInformation("Order {OrderId} created for customer {CustomerId}", e.OrderId, e.CustomerId);
        return Task.CompletedTask;
    }
}
 
public sealed class OrderConfirmedEventHandler(ILogger<OrderConfirmedEventHandler> log)
    : INotificationHandler<OrderConfirmedEvent>
{
    public Task Handle(OrderConfirmedEvent e, CancellationToken ct)
    {
        log.LogInformation("Order {OrderId} confirmed. Total: {Total}", e.OrderId, e.TotalAmount);
        return Task.CompletedTask;
    }
}
 
public sealed class OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> log)
    : INotificationHandler<OrderCancelledEvent>
{
    public Task Handle(OrderCancelledEvent e, CancellationToken ct)
    {
        log.LogWarning("Order {OrderId} cancelled. Reason: {Reason}", e.OrderId, e.Reason);
        return Task.CompletedTask;
    }
}
