using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
 
namespace RetailStore.Api.Features.Orders.Domain;
 
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }          // DB column: UnitPrice decimal(18,2)
    public string UnitPriceCurrency { get; private set; } = "USD";  // DB column: UnitPriceCurrency nvarchar(3)
 
    private OrderItem() { } // EF Core
 
    // ─── Computed (not in DB) ───────────────────────────────
    public decimal Subtotal => UnitPrice * Quantity;
    public Money UnitPriceMoney => new(UnitPrice, UnitPriceCurrency);
    public Money SubtotalMoney => new(Subtotal, UnitPriceCurrency);
 
    // ─── Factory ────────────────────────────────────────────
    internal static OrderItem Create(
        Guid orderId, Guid productId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
 
        if (unitPrice.Amount <= 0)
            throw new DomainException(OrderItemErrors.InvalidUnitPrice());
 
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice.Amount,
            UnitPriceCurrency = unitPrice.Currency
        };
    }
 
    // ─── Mutations ──────────────────────────────────────────
    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
        Quantity += amount;
    }
 
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
        Quantity = newQuantity;
    }
 
    internal void UpdateUnitPrice(Money newUnitPrice)
    {
        if (newUnitPrice.Amount <= 0)
            throw new DomainException(OrderItemErrors.InvalidUnitPrice());
        UnitPrice = newUnitPrice.Amount;
        UnitPriceCurrency = newUnitPrice.Currency;
    }
}