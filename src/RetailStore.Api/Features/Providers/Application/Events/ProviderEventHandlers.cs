using MediatR;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Providers.Domain;
 
namespace RetailStore.Api.Features.Providers.Application.Events;
 
public sealed class ProviderRegisteredEventHandler(ILogger<ProviderRegisteredEventHandler> log)
    : INotificationHandler<ProviderRegisteredEvent>
{
    public Task Handle(ProviderRegisteredEvent e, CancellationToken ct)
    {
        log.LogInformation("Provider registered: {Company} ({Email})", e.CompanyName, e.Email);
        return Task.CompletedTask;
    }
}
 
public sealed class ProviderDeactivatedEventHandler(ILogger<ProviderDeactivatedEventHandler> log)
    : INotificationHandler<ProviderDeactivatedEvent>
{
    public Task Handle(ProviderDeactivatedEvent e, CancellationToken ct)
    {
        log.LogWarning("Provider deactivated: {Company}", e.CompanyName);
        // TODO: Find alternative providers for associated products
        // TODO: Notify purchasing team
        return Task.CompletedTask;
    }
}
 
// ─── Cross-module: React to LowStockAlert ───────────────────
// When inventory is low, notify the providers who supply that product
public sealed class NotifyProviderOnLowStockHandler(ILogger<NotifyProviderOnLowStockHandler> log)
    : INotificationHandler<LowStockAlertEvent>
{
    public Task Handle(LowStockAlertEvent e, CancellationToken ct)
    {
        log.LogWarning(
            "Low stock alert for product {ProductId} (qty: {Qty}, threshold: {Threshold}) — notifying providers",
            e.ProductId, e.CurrentQuantity, e.Threshold);
        // TODO: Load providers who supply this product
        // TODO: Send reorder notification email
        // TODO: Create purchase order draft
        return Task.CompletedTask;
    }
}