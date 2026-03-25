using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Inventory.Domain;
 
// ─── Lifecycle ──────────────────────────────────────────────
public sealed record InventoryItemCreatedEvent(
    Guid InventoryItemId, Guid ProductId,
    int InitialQuantity, int ReorderThreshold) : DomainEvent
{ public override string EventType => "InventoryItemCreated"; }
 
// ─── Stock Movement ─────────────────────────────────────────
public sealed record StockAddedEvent(
    Guid InventoryItemId, Guid ProductId,
    int QuantityAdded, int NewTotal) : DomainEvent
{ public override string EventType => "StockAdded"; }
 
public sealed record StockRemovedEvent(
    Guid InventoryItemId, Guid ProductId,
    int QuantityRemoved, int NewTotal) : DomainEvent
{ public override string EventType => "StockRemoved"; }
 
public sealed record StockAdjustedEvent(
    Guid InventoryItemId, Guid ProductId,
    int OldQuantity, int NewQuantity, string Reason) : DomainEvent
{ public override string EventType => "StockAdjusted"; }
 
// ─── Reservations ───────────────────────────────────────────
public sealed record StockReservedEvent(
    Guid InventoryItemId, Guid ProductId,
    int QuantityReserved, int AvailableAfter) : DomainEvent
{ public override string EventType => "StockReserved"; }
 
public sealed record StockReservationReleasedEvent(
    Guid InventoryItemId, Guid ProductId,
    int QuantityReleased, int AvailableAfter) : DomainEvent
{ public override string EventType => "StockReservationReleased"; }
 
public sealed record StockFulfilledEvent(
    Guid InventoryItemId, Guid ProductId,
    int QuantityFulfilled, int NewTotal) : DomainEvent
{ public override string EventType => "StockFulfilled"; }
 
// ─── Alerts ─────────────────────────────────────────────────
public sealed record LowStockAlertEvent(
    Guid InventoryItemId, Guid ProductId,
    int CurrentQuantity, int Threshold) : DomainEvent
{ public override string EventType => "LowStockAlert"; }
 
public sealed record OutOfStockAlertEvent(
    Guid InventoryItemId, Guid ProductId) : DomainEvent
{ public override string EventType => "OutOfStockAlert"; }
 
public sealed record StockRecoveredEvent(
    Guid InventoryItemId, Guid ProductId,
    int CurrentQuantity) : DomainEvent
{ public override string EventType => "StockRecovered"; }
 
public sealed record ReorderThresholdUpdatedEvent(
    Guid InventoryItemId, Guid ProductId,
    int NewThreshold) : DomainEvent
{ public override string EventType => "ReorderThresholdUpdated"; }