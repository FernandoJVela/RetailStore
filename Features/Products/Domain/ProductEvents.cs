using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Domain;

public record ProductCreatedEvent(
    Guid ProductId, string Name,
    string Sku, decimal Price) : DomainEvent
{
    public override string EventType => "ProductCreated";
}

public record ProductPriceChangedEvent(
    Guid ProductId, decimal OldPrice,
    decimal NewPrice) : DomainEvent
{
    public override string EventType => "ProductPriceChanged";
}

public record ProductDeactivatedEvent(
    Guid ProductId) : DomainEvent
{
    public override string EventType => "ProductDeactivated";
}
