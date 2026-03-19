using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Orders.Domain;

public class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public Money Total { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    private Order() { } // EF Core
    
    public static Order Create(Guid customerId, DateTime? orderDate)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(OrderErrors.CustomerNotFound(customerId));

        if (orderDate > DateTime.UtcNow.AddMinutes(5))
            throw new DomainException(OrderErrors.InvalidOrderDate());

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            Status = OrderStatus.Draft,
            Total = Money.Zero,
            CreatedAt = DateTime.UtcNow
        };
        
        order.Raise(new OrderCreatedEvent(order.Id, order.CustomerId, order.Total));
        
        return order;
    }
    
    public void AddItem(Guid productId, int quantity, Money unitPrice)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var orderItem = OrderItem.Create(productId, quantity, unitPrice);
            _items.Add(orderItem);
        }
        
        RecalculateTotal();
        Raise(new OrderItemAddedEvent(Id, productId, quantity, unitPrice));
    }
    
    public void RemoveItem(Guid productId)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException(OrderErrors.InvalidOrderItem(productId));
        
        _items.Remove(item);
        RecalculateTotal();
        
        Raise(new OrderItemRemovedEvent(Id, productId));
    }
    
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException(OrderErrors.InvalidOrderItem(productId));
        
        item.UpdateQuantity(newQuantity);
        RecalculateTotal();
    }
    
    public void Confirm()
    {
        if (!_items.Any())
            throw new DomainException(OrderErrors.CannotCompleteEmptyOrder());
        
        if (Status != OrderStatus.Draft && Status != OrderStatus.Pending)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        Status = OrderStatus.Confirmed;
        Raise(new OrderConfirmedEvent(Id, CustomerId, Total));
    }
    
    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        Raise(new OrderCompletedEvent(Id, CustomerId, Total));
    }
    
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        if (Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.InvalidOrderStatusForModification(Status));
        
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        
        Raise(new OrderCancelledEvent(Id, CustomerId, reason));
    }
    
    private void RecalculateTotal()
    {
        var totalAmount = _items.Sum(i => i.Subtotal.Amount);
        Total = new Money(totalAmount, "USD");
    }
    
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}