using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Products.Domain;

public record ProductCreatedEvent(
    Guid ProductId, string Name,
    string Sku, Money Price) : DomainEvent
{
    public override string EventType => "ProductCreated";
}

public record ProductPriceChangedEvent(
    Guid ProductId, Money OldPrice,
    Money NewPrice) : DomainEvent
{
    public override string EventType => "ProductPriceChanged";
}

public record ProductDeactivatedEvent(
    Guid ProductId) : DomainEvent
{
    public override string EventType => "ProductDeactivated";
}
