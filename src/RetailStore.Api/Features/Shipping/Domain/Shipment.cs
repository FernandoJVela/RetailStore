using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Domain;
 
public sealed class Shipment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ShipmentStatus Status { get; private set; }
 
    // Carrier
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime? EstimatedDelivery { get; private set; }
 
    // Address snapshot
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string Country { get; private set; } = string.Empty;
 
    // Cost & weight
    public decimal ShippingCost { get; private set; }
    public string CostCurrency { get; private set; } = "USD";
    public decimal? TotalWeightKg { get; private set; }
 
    // Timestamps
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? Notes { get; private set; }
 
    private readonly List<ShipmentItem> _items = new();
    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();
 
    private Shipment() { } // EF Core
 
    // ─── Computed ───────────────────────────────────────────
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}".Trim();
    public int ItemCount => _items.Count;
    public int TotalQuantity => _items.Sum(i => i.Quantity);
 
    // ─── Factory ────────────────────────────────────────────
    public static Shipment Create(
        Guid orderId, Guid customerId,
        string street, string city, string country,
        string? state = null, string? zipCode = null)
    {
        if (orderId == Guid.Empty)
            throw new DomainException(ShippingErrors.InvalidOrderId());
        if (customerId == Guid.Empty)
            throw new DomainException(ShippingErrors.InvalidCustomerId());
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException(ShippingErrors.InvalidAddress("Street is required."));
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(ShippingErrors.InvalidAddress("City is required."));
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException(ShippingErrors.InvalidAddress("Country is required."));
 
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Status = ShipmentStatus.Pending,
            Street = street.Trim(),
            City = city.Trim(),
            State = state?.Trim(),
            ZipCode = zipCode?.Trim(),
            Country = country.Trim(),
            ShippingCost = 0,
            CostCurrency = "USD"
        };
 
        shipment.Raise(new ShipmentCreatedEvent(
            shipment.Id, orderId, customerId));
 
        return shipment;
    }
 
    // ─── Items ──────────────────────────────────────────────
    public void AddItem(Guid productId, string productName, int quantity, decimal? weightKg = null)
    {
        GuardModifiable();
 
        if (_items.Any(i => i.ProductId == productId))
            throw new DomainException(ShippingErrors.DuplicateItem(productId));
 
        var item = ShipmentItem.Create(Id, productId, productName, quantity, weightKg);
        _items.Add(item);
        RecalculateWeight();
        Touch();
    }
 
    public void RemoveItem(Guid productId)
    {
        GuardModifiable();
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new DomainException(ShippingErrors.ItemNotFound(productId));
        _items.Remove(item);
        RecalculateWeight();
        Touch();
    }
 
    // ─── Carrier Assignment ─────────────────────────────────
    public void AssignCarrier(string carrier, string trackingNumber, DateTime? estimatedDelivery = null)
    {
        if (Status != ShipmentStatus.Pending && Status != ShipmentStatus.Processing)
            throw new DomainException(ShippingErrors.InvalidStatusForCarrierAssignment(Status));
 
        if (string.IsNullOrWhiteSpace(carrier))
            throw new DomainException(ShippingErrors.InvalidCarrier());
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException(ShippingErrors.InvalidTrackingNumber());
 
        Carrier = carrier.Trim();
        TrackingNumber = trackingNumber.Trim();
        EstimatedDelivery = estimatedDelivery;
        Status = ShipmentStatus.Processing;
        Touch();
 
        Raise(new CarrierAssignedEvent(Id, OrderId, Carrier, TrackingNumber));
    }
 
    public void SetShippingCost(decimal cost, string currency = "USD")
    {
        if (cost < 0)
            throw new DomainException(ShippingErrors.InvalidCost());
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException(ShippingErrors.InvalidCurrency());
 
        ShippingCost = cost;
        CostCurrency = currency.ToUpperInvariant();
        Touch();
    }
 
    // ─── Status Transitions ─────────────────────────────────
    public void MarkShipped()
    {
        if (Status != ShipmentStatus.Processing)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.Shipped));
 
        if (string.IsNullOrEmpty(Carrier) || string.IsNullOrEmpty(TrackingNumber))
            throw new DomainException(ShippingErrors.CarrierRequiredBeforeShipping());
 
        if (!_items.Any())
            throw new DomainException(ShippingErrors.EmptyShipment());
 
        Status = ShipmentStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        Touch();
        IncrementVersion();
 
        Raise(new ShipmentShippedEvent(
            Id, OrderId, CustomerId, Carrier!, TrackingNumber!));
    }
 
    public void MarkInTransit()
    {
        if (Status != ShipmentStatus.Shipped)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.InTransit));
 
        Status = ShipmentStatus.InTransit;
        Touch();
 
        Raise(new ShipmentInTransitEvent(Id, OrderId, TrackingNumber!));
    }
 
    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.Shipped && Status != ShipmentStatus.InTransit)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.Delivered));
 
        Status = ShipmentStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        Touch();
        IncrementVersion();
 
        Raise(new ShipmentDeliveredEvent(Id, OrderId, CustomerId, DeliveredAt.Value));
    }
 
    public void MarkFailed(string reason)
    {
        if (Status != ShipmentStatus.Shipped && Status != ShipmentStatus.InTransit)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.Failed));
 
        Status = ShipmentStatus.Failed;
        Notes = reason;
        Touch();
 
        Raise(new ShipmentFailedEvent(Id, OrderId, reason));
    }
 
    public void MarkReturned(string reason)
    {
        if (Status != ShipmentStatus.Failed && Status != ShipmentStatus.Delivered)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.Returned));
 
        Status = ShipmentStatus.Returned;
        Notes = reason;
        Touch();
 
        Raise(new ShipmentReturnedEvent(Id, OrderId, reason));
    }
 
    public void Cancel(string reason)
    {
        if (Status == ShipmentStatus.Delivered || Status == ShipmentStatus.Returned)
            throw new DomainException(
                ShippingErrors.InvalidStatusTransition(Status, ShipmentStatus.Cancelled));
 
        Status = ShipmentStatus.Cancelled;
        Notes = reason;
        Touch();
 
        Raise(new ShipmentCancelledEvent(Id, OrderId, reason));
    }
 
    public void AddNote(string note)
    {
        Notes = string.IsNullOrEmpty(Notes) ? note : $"{Notes}\n{note}";
        Touch();
    }
 
    // ─── Helpers ────────────────────────────────────────────
    private void GuardModifiable()
    {
        if (Status is ShipmentStatus.Shipped or ShipmentStatus.InTransit
            or ShipmentStatus.Delivered or ShipmentStatus.Returned or ShipmentStatus.Cancelled)
            throw new DomainException(ShippingErrors.CannotModifyShipment(Status));
    }
 
    private void RecalculateWeight()
    {
        var total = _items.Where(i => i.WeightKg.HasValue).Sum(i => i.WeightKg!.Value);
        TotalWeightKg = total > 0 ? total : null;
    }
}