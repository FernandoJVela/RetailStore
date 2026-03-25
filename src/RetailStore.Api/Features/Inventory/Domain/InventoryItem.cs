using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Inventory.Domain;
 
public sealed class InventoryItem : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderThreshold { get; private set; }
 
    private InventoryItem() { } // EF Core
 
    // ─── Computed (not in DB) ───────────────────────────────
    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public bool IsLowStock => QuantityOnHand <= ReorderThreshold;
    public bool IsOutOfStock => AvailableQuantity <= 0;
 
    public string StockStatus => IsOutOfStock
        ? "OutOfStock"
        : IsLowStock
            ? "LowStock"
            : "InStock";
 
    // ─── Factory ────────────────────────────────────────────
    public static InventoryItem Create(
        Guid productId, int initialQuantity, int reorderThreshold = 10)
    {
        if (productId == Guid.Empty)
            throw new DomainException(InventoryErrors.InvalidProductId());
 
        if (initialQuantity < 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        if (reorderThreshold < 0)
            throw new DomainException(InventoryErrors.InvalidReorderThreshold());
 
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            QuantityOnHand = initialQuantity,
            ReservedQuantity = 0,
            ReorderThreshold = reorderThreshold
        };
 
        item.Raise(new InventoryItemCreatedEvent(
            item.Id, productId, initialQuantity, reorderThreshold));
 
        return item;
    }
 
    // ─── Stock Operations ───────────────────────────────────
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        QuantityOnHand += quantity;
        Touch();
        IncrementVersion();
 
        Raise(new StockAddedEvent(Id, ProductId, quantity, QuantityOnHand));
 
        // Check if we recovered from low stock
        if (!IsLowStock && !IsOutOfStock)
            Raise(new StockRecoveredEvent(Id, ProductId, QuantityOnHand));
    }
 
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        if (quantity > QuantityOnHand)
            throw new DomainException(
                InventoryErrors.InsufficientStock(ProductId, AvailableQuantity, quantity));
 
        QuantityOnHand -= quantity;
        Touch();
        IncrementVersion();
 
        Raise(new StockRemovedEvent(Id, ProductId, quantity, QuantityOnHand));
 
        CheckLowStock();
    }
 
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        if (quantity > AvailableQuantity)
            throw new DomainException(
                InventoryErrors.InsufficientStock(ProductId, AvailableQuantity, quantity));
 
        ReservedQuantity += quantity;
        Touch();
 
        Raise(new StockReservedEvent(Id, ProductId, quantity, AvailableQuantity));
 
        CheckLowStock();
    }
 
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        // Don't release more than what's reserved
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        Touch();
 
        Raise(new StockReservationReleasedEvent(Id, ProductId, quantity, AvailableQuantity));
    }
 
    public void FulfillReservation(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        if (quantity > ReservedQuantity)
            throw new DomainException(
                InventoryErrors.FulfillExceedsReservation(ProductId, ReservedQuantity, quantity));
 
        ReservedQuantity -= quantity;
        QuantityOnHand -= quantity;
        Touch();
        IncrementVersion();
 
        Raise(new StockFulfilledEvent(Id, ProductId, quantity, QuantityOnHand));
 
        CheckLowStock();
    }
 
    // ─── Adjustments ────────────────────────────────────────
    public void AdjustQuantity(int newQuantity, string reason)
    {
        if (newQuantity < 0)
            throw new DomainException(InventoryErrors.NegativeQuantity());
 
        var oldQuantity = QuantityOnHand;
        QuantityOnHand = newQuantity;
 
        // Reset reservations if new quantity can't cover them
        if (ReservedQuantity > QuantityOnHand)
            ReservedQuantity = QuantityOnHand;
 
        Touch();
        IncrementVersion();
 
        Raise(new StockAdjustedEvent(Id, ProductId, oldQuantity, newQuantity, reason));
 
        CheckLowStock();
    }
 
    public void UpdateReorderThreshold(int newThreshold)
    {
        if (newThreshold < 0)
            throw new DomainException(InventoryErrors.InvalidReorderThreshold());
 
        ReorderThreshold = newThreshold;
        Touch();
 
        Raise(new ReorderThresholdUpdatedEvent(Id, ProductId, newThreshold));
 
        CheckLowStock();
    }
 
    // ─── Helpers ────────────────────────────────────────────
    private void CheckLowStock()
    {
        if (IsOutOfStock)
            Raise(new OutOfStockAlertEvent(Id, ProductId));
        else if (IsLowStock)
            Raise(new LowStockAlertEvent(Id, ProductId, QuantityOnHand, ReorderThreshold));
    }
}