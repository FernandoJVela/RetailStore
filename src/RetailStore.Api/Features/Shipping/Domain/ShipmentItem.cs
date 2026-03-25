using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Domain;
 
public sealed class ShipmentItem : Entity
{
    public Guid ShipmentId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal? WeightKg { get; private set; }
 
    private ShipmentItem() { } // EF Core
 
    internal static ShipmentItem Create(
        Guid shipmentId, Guid productId, string productName,
        int quantity, decimal? weightKg = null)
    {
        if (quantity <= 0)
            throw new DomainException(ShippingErrors.InvalidQuantity());
 
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException(ShippingErrors.InvalidProductName());
 
        return new ShipmentItem
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            WeightKg = weightKg
        };
    }
 
    internal void UpdateWeight(decimal weightKg)
    {
        if (weightKg < 0)
            throw new DomainException(ShippingErrors.InvalidWeight());
        WeightKg = weightKg;
    }
}