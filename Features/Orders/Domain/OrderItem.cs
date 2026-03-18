using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Orders.Domain;

public class OrderItem : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string UnitPriceCurrency { get; private set; }
    public Money Subtotal { get; private set; }
    
    private OrderItem() { } // EF Core
    
    internal static OrderItem Create(
        Guid productId, 
        string productName, 
        int quantity, 
        Money unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException(OrderItemErrors.InvalidProductName());

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice.Amount,
            UnitPriceCurrency = unitPrice.Currency,
            Subtotal = new Money(unitPrice.Amount * quantity, unitPrice.Currency)
        };
    }
    
    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
        
        Quantity += amount;
        CalculateSubtotal();
    }
    
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException(OrderItemErrors.InvalidQuantity());
        
        Quantity = newQuantity;
        CalculateSubtotal();
    }
    
    internal void UpdateUnitPrice(Money newUnitPrice)
    {
        if (newUnitPrice.Amount <= 0)
            throw new DomainException(OrderItemErrors.InvalidUnitPrice());
        
        UnitPrice = newUnitPrice.Amount;
        UnitPriceCurrency = newUnitPrice.Currency;
        CalculateSubtotal();
    }
    
    private void CalculateSubtotal()
    {
        Subtotal = new Money(UnitPrice * Quantity, UnitPriceCurrency);
    }
}