using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Products.Domain;
 
// IMPORTANT: Domain events are serialized to JSON in the Outbox.
// Use ONLY primitives. Never put value objects (Money) in events.
 
public sealed record ProductCreatedEvent(
    Guid ProductId, string Name, string Sku,
    decimal PriceAmount, string PriceCurrency) : DomainEvent
{ public override string EventType => "ProductCreated"; }
 
public sealed record ProductUpdatedEvent(
    Guid ProductId, string Name, string Category) : DomainEvent
{ public override string EventType => "ProductUpdated"; }
 
public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPriceAmount, string OldPriceCurrency,
    decimal NewPriceAmount, string NewPriceCurrency) : DomainEvent
{ public override string EventType => "ProductPriceChanged"; }
 
public sealed record ProductDeactivatedEvent(
    Guid ProductId, string Name, string Sku) : DomainEvent
{ public override string EventType => "ProductDeactivated"; }
 
public sealed record ProductReactivatedEvent(
    Guid ProductId, string Name, string Sku) : DomainEvent
{ public override string EventType => "ProductReactivated"; }