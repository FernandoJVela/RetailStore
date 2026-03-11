using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Products.Domain;

namespace RetailStore.Api.Features.Products.Application.Events;

// This handler runs when the Outbox publishes the event.
// It can trigger read model projections,
// notify other modules, send emails, etc.
public class ProductCreatedEventHandler
    : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        ProductCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Product created: {Name} ({Sku}) at ${Price}",
            notification.Name, notification.Sku,
            notification.Price);

        // TODO: Update read model / search index
        // TODO: Notify Inventory module to create stock entry
        return Task.CompletedTask;
    }
}
