using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Orders.Domain;
 
public sealed record OrderCreatedEvent(
    Guid OrderId, Guid CustomerId) : DomainEvent
{ public override string EventType => "OrderCreated"; }
 
public sealed record OrderConfirmedEvent(
    Guid OrderId, Guid CustomerId, decimal TotalAmount) : DomainEvent
{ public override string EventType => "OrderConfirmed"; }
 
public sealed record OrderCompletedEvent(
    Guid OrderId, Guid CustomerId, decimal TotalAmount) : DomainEvent
{ public override string EventType => "OrderCompleted"; }
 
public sealed record OrderCancelledEvent(
    Guid OrderId, Guid CustomerId, string Reason) : DomainEvent
{ public override string EventType => "OrderCancelled"; }
 
public sealed record OrderItemAddedEvent(
    Guid OrderId, Guid ProductId, int Quantity,
    decimal UnitPriceAmount, string UnitPriceCurrency) : DomainEvent
{ public override string EventType => "OrderItemAdded"; }
 
public sealed record OrderItemRemovedEvent(
    Guid OrderId, Guid ProductId) : DomainEvent
{ public override string EventType => "OrderItemRemoved"; }