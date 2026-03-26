using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Products.Domain;
 
namespace RetailStore.Api.Features.Products.Application.Events;
 
public sealed class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> log)
    : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Product created: {Name} ({Sku}) at {Amount} {Currency}",
            e.Name, e.Sku, e.PriceAmount, e.PriceCurrency);
        // TODO: Notify Inventory module to create stock entry
        // TODO: Update search index
        return Task.CompletedTask;
    }
}
 
public sealed class ProductPriceChangedEventHandler(ILogger<ProductPriceChangedEventHandler> log)
    : INotificationHandler<ProductPriceChangedEvent>
{
    public Task Handle(ProductPriceChangedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Product {ProductId} price changed: {OldAmount} {OldCurrency} → {NewAmount} {NewCurrency}",
            e.ProductId, e.OldPriceAmount, e.OldPriceCurrency,
            e.NewPriceAmount, e.NewPriceCurrency);
        // TODO: Notify customers who have this product in their cart
        // TODO: Update cached prices
        return Task.CompletedTask;
    }
}
 
public sealed class ProductDeactivatedEventHandler(ILogger<ProductDeactivatedEventHandler> log)
    : INotificationHandler<ProductDeactivatedEvent>
{
    public Task Handle(ProductDeactivatedEvent e, CancellationToken ct)
    {
        log.LogWarning(
            "Product deactivated: {Name} ({Sku})",
            e.Name, e.Sku);
        // TODO: Remove from search index
        // TODO: Notify inventory module
        return Task.CompletedTask;
    }
}
 
public sealed class ProductReactivatedEventHandler(ILogger<ProductReactivatedEventHandler> log)
    : INotificationHandler<ProductReactivatedEvent>
{
    public Task Handle(ProductReactivatedEvent e, CancellationToken ct)
    {
        log.LogInformation(
            "Product reactivated: {Name} ({Sku})",
            e.Name, e.Sku);
        // TODO: Add back to search index
        return Task.CompletedTask;
    }
}