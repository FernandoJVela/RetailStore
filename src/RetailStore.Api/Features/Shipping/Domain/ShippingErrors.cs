using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Domain;
 
public static class ShippingErrors
{
    // ─── Lookup ────────────────────────────────────────────
    public static DomainError NotFound(Guid id) => new(
        "SHIPMENT_NOT_FOUND", $"Shipment '{id}' not found.", DomainErrorType.NotFound);
    public static DomainError NotFoundByOrder(Guid orderId) => new(
        "SHIPMENT_NOT_FOUND_BY_ORDER", $"No shipment for order '{orderId}'.", DomainErrorType.NotFound);
    public static DomainError NotFoundByTracking(string tracking) => new(
        "SHIPMENT_NOT_FOUND_BY_TRACKING", $"No shipment with tracking '{tracking}'.", DomainErrorType.NotFound);
    public static DomainError ItemNotFound(Guid productId) => new(
        "SHIPMENT_ITEM_NOT_FOUND", $"No item with product '{productId}' in this shipment.", DomainErrorType.NotFound);
 
    // ─── Conflict ──────────────────────────────────────────
    public static DomainError AlreadyExists(Guid orderId) => new(
        "SHIPMENT_ALREADY_EXISTS", $"A shipment for order '{orderId}' already exists.", DomainErrorType.Conflict);
    public static DomainError DuplicateItem(Guid productId) => new(
        "SHIPMENT_DUPLICATE_ITEM", $"Product '{productId}' is already in this shipment.", DomainErrorType.Conflict);
 
    // ─── Status ────────────────────────────────────────────
    public static DomainError InvalidStatusTransition(ShipmentStatus from, ShipmentStatus to) => new(
        "SHIPMENT_INVALID_STATUS_TRANSITION", $"Cannot transition from '{from}' to '{to}'.", DomainErrorType.BusinessRule);
    public static DomainError InvalidStatusForCarrierAssignment(ShipmentStatus status) => new(
        "SHIPMENT_INVALID_STATUS_FOR_CARRIER", $"Cannot assign carrier in '{status}' status.", DomainErrorType.BusinessRule);
    public static DomainError CannotModifyShipment(ShipmentStatus status) => new(
        "SHIPMENT_CANNOT_MODIFY", $"Cannot modify shipment in '{status}' status.", DomainErrorType.BusinessRule);
    public static DomainError EmptyShipment() => new(
        "SHIPMENT_EMPTY", "Cannot ship without items.", DomainErrorType.BusinessRule);
    public static DomainError CarrierRequiredBeforeShipping() => new(
        "SHIPMENT_CARRIER_REQUIRED", "Carrier and tracking number must be assigned before shipping.", DomainErrorType.BusinessRule);
 
    // ─── Validation ────────────────────────────────────────
    public static DomainError InvalidOrderId() => new(
        "SHIPMENT_INVALID_ORDER_ID", "A valid order ID is required.", DomainErrorType.Validation);
    public static DomainError InvalidCustomerId() => new(
        "SHIPMENT_INVALID_CUSTOMER_ID", "A valid customer ID is required.", DomainErrorType.Validation);
    public static DomainError InvalidAddress(string reason) => new(
        "SHIPMENT_INVALID_ADDRESS", $"Invalid address: {reason}", DomainErrorType.Validation);
    public static DomainError InvalidCarrier() => new(
        "SHIPMENT_INVALID_CARRIER", "Carrier name is required.", DomainErrorType.Validation);
    public static DomainError InvalidTrackingNumber() => new(
        "SHIPMENT_INVALID_TRACKING", "Tracking number is required.", DomainErrorType.Validation);
    public static DomainError InvalidQuantity() => new(
        "SHIPMENT_INVALID_QUANTITY", "Quantity must be positive.", DomainErrorType.Validation);
    public static DomainError InvalidProductName() => new(
        "SHIPMENT_INVALID_PRODUCT_NAME", "Product name is required.", DomainErrorType.Validation);
    public static DomainError InvalidWeight() => new(
        "SHIPMENT_INVALID_WEIGHT", "Weight cannot be negative.", DomainErrorType.Validation);
    public static DomainError InvalidCost() => new(
        "SHIPMENT_INVALID_COST", "Shipping cost cannot be negative.", DomainErrorType.Validation);
    public static DomainError InvalidCurrency() => new(
        "SHIPMENT_INVALID_CURRENCY", "Currency must be 3-letter ISO code.", DomainErrorType.Validation);
}