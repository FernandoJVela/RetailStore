using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Orders.Domain;

public record OrderCreatedEvent(
    Guid OrderId, Guid CustomerId,
    Money Total) : DomainEvent
{
    public override string EventType => "OrderCreated";
}

public record OrderConfirmedEvent(
    Guid OrderId, Guid CustomerId,
    Money Total) : DomainEvent
{
    public override string EventType => "OrderConfirmed";
}

public record OrderCompletedEvent(
    Guid OrderId, Guid CustomerId,
    Money Total) : DomainEvent
{
    public override string EventType => "OrderCompleted";
}

public record OrderItemAddedEvent(
    Guid OrderId, Guid ProductId,
    int Quantity, Money UnitPrice) : DomainEvent
{
    public override string EventType => "OrderItemAdded";
}

public record OrderItemUpdatedEvent(
    Guid OrderId, Guid ProductId,
    int Quantity, Money UnitPrice) : DomainEvent
{
    public override string EventType => "OrderItemUpdated";
}

public record OrderItemRemovedEvent(
    Guid OrderId, Guid ProductId) : DomainEvent
{
    public override string EventType => "OrderItemRemoved";
}

public record OrderCancelledEvent(
    Guid OrderId, Guid CustomerId, string reason) : DomainEvent
{
    public override string EventType => "OrderCancelled";
}