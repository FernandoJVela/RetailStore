using MediatR;
using RetailStore.Api.Features.Orders.Domain;

namespace RetailStore.Api.Features.Orders.Application.Events;

// This handler runs when the Outbox publishes the event.
// It can trigger read model projections,
// notify other modules, send emails, etc.
public class OrderCreatedEventHandler
    : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        ILogger<OrderCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        OrderCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order created: {OrderId} for customer ({CustomerId}) at ${Total}",
            notification.OrderId, notification.CustomerId,
            notification.Total);

        // TODO: Update read model / search index
        // TODO: Notify Inventory module to create stock entry
        return Task.CompletedTask;
    }
}
