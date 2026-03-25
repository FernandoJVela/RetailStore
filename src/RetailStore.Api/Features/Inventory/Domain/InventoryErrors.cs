using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Inventory.Domain;
 
public static class InventoryErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound(Guid id) => new(
        "INVENTORY_NOT_FOUND",
        $"Inventory item '{id}' does not exist.",
        DomainErrorType.NotFound);
 
    public static DomainError NotFoundByProduct(Guid productId) => new(
        "INVENTORY_NOT_FOUND_BY_PRODUCT",
        $"No inventory record for product '{productId}'.",
        DomainErrorType.NotFound);
 
    // ─── Conflict ──────────────────────────────────────────
    public static DomainError AlreadyExists(Guid productId) => new(
        "INVENTORY_ALREADY_EXISTS",
        $"An inventory record for product '{productId}' already exists.",
        DomainErrorType.Conflict);
 
    // ─── Business Rules ────────────────────────────────────
    public static DomainError InsufficientStock(Guid productId, int available, int requested) => new(
        "INVENTORY_INSUFFICIENT_STOCK",
        $"Insufficient stock for product '{productId}'. Available: {available}, Requested: {requested}.",
        DomainErrorType.BusinessRule);
 
    public static DomainError NegativeQuantity() => new(
        "INVENTORY_NEGATIVE_QUANTITY",
        "Quantity must be a positive number.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidReorderThreshold() => new(
        "INVENTORY_INVALID_REORDER_THRESHOLD",
        "Reorder threshold cannot be negative.",
        DomainErrorType.Validation);
 
    public static DomainError InvalidProductId() => new(
        "INVENTORY_INVALID_PRODUCT_ID",
        "A valid product ID is required.",
        DomainErrorType.Validation);
 
    public static DomainError FulfillExceedsReservation(Guid productId, int reserved, int requested) => new(
        "INVENTORY_FULFILL_EXCEEDS_RESERVATION",
        $"Cannot fulfill {requested} units for product '{productId}'. Only {reserved} reserved.",
        DomainErrorType.BusinessRule);
}