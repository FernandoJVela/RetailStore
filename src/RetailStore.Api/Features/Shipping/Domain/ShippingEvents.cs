using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Domain;
 
public sealed record ShipmentCreatedEvent(
    Guid ShipmentId, Guid OrderId, Guid CustomerId) : DomainEvent
{ public override string EventType => "ShipmentCreated"; }
 
public sealed record CarrierAssignedEvent(
    Guid ShipmentId, Guid OrderId,
    string Carrier, string TrackingNumber) : DomainEvent
{ public override string EventType => "CarrierAssigned"; }
 
public sealed record ShipmentShippedEvent(
    Guid ShipmentId, Guid OrderId, Guid CustomerId,
    string Carrier, string TrackingNumber) : DomainEvent
{ public override string EventType => "ShipmentShipped"; }
 
public sealed record ShipmentInTransitEvent(
    Guid ShipmentId, Guid OrderId, string TrackingNumber) : DomainEvent
{ public override string EventType => "ShipmentInTransit"; }
 
public sealed record ShipmentDeliveredEvent(
    Guid ShipmentId, Guid OrderId, Guid CustomerId,
    DateTime DeliveredAt) : DomainEvent
{ public override string EventType => "ShipmentDelivered"; }
 
public sealed record ShipmentFailedEvent(
    Guid ShipmentId, Guid OrderId, string Reason) : DomainEvent
{ public override string EventType => "ShipmentFailed"; }
 
public sealed record ShipmentReturnedEvent(
    Guid ShipmentId, Guid OrderId, string Reason) : DomainEvent
{ public override string EventType => "ShipmentReturned"; }
 
public sealed record ShipmentCancelledEvent(
    Guid ShipmentId, Guid OrderId, string Reason) : DomainEvent
{ public override string EventType => "ShipmentCancelled"; }