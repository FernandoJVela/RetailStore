using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;
 
namespace RetailStore.Api.Features.Orders.Domain;
 
public class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
 
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
 
    private Order() { } // EF Core
 
    // ─── Computed (not in DB — calculated from line items) ──
    public decimal TotalAmount => _items.Sum(i => i.UnitPrice * i.Quantity);
    public Money Total => new(TotalAmount, _items.FirstOrDefault()?.UnitPriceCurrency ?? "USD");
 
    // ─── Factory ────────────────────────────────────────────
    public static Order Create(Guid customerId, DateTime? orderDate = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(OrderErrors.CustomerNotFound(customerId));
 
        if (orderDate.HasValue && orderDate > DateTime.UtcNow.AddMinutes(5))
            throw new DomainException(OrderErrors.InvalidOrderDate());
 
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            Status = OrderStatus.Draft
        };
 
        order.Raise(new OrderCreatedEvent(order.Id, order.CustomerId));
        return order;
    }
 
    // ─── Item Management ────────────────────────────────────
    public void AddItem(Guid productId, int quantity, Money unitPrice)
    {
        GuardModifiable();
 
        if (unitPrice is null || unitPrice.Amount <= 0)
            throw new DomainException(OrderErrors.InvalidOrderItemPrice(productId));
 
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
 
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var orderItem = OrderItem.Create(Id, productId, quantity, unitPrice);
            _items.Add(orderItem);
        }
 
        Touch();
        Raise(new OrderItemAddedEvent(Id, productId, quantity, unitPrice.Amount, unitPrice.Currency));
    }
 
    public void RemoveItem(Guid productId)
    {
        GuardModifiable();
 
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new DomainException(OrderErrors.InvalidOrderItem(productId));
 
        _items.Remove(item);
        Touch();
        Raise(new OrderItemRemovedEvent(Id, productId));
    }
 
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        GuardModifiable();
 
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new DomainException(OrderErrors.InvalidOrderItem(productId));
 
        item.UpdateQuantity(newQuantity);
        Touch();
    }
 
    // ─── Status Transitions ─────────────────────────────────
    public void Confirm()
    {
        if (!_items.Any())
            throw new DomainException(OrderErrors.CannotCompleteEmptyOrder());
 
        if (Status != OrderStatus.Draft && Status != OrderStatus.Pending)
            throw new DomainException(
                OrderErrors.InvalidOrderStatusTransition(Status, OrderStatus.Confirmed));
 
        Status = OrderStatus.Confirmed;
        Touch();
        Raise(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
    }
 
    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException(
                OrderErrors.InvalidOrderStatusTransition(Status, OrderStatus.Completed));
 
        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Touch();
        Raise(new OrderCompletedEvent(Id, CustomerId, TotalAmount));
    }
 
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException(
                OrderErrors.InvalidOrderStatusTransition(Status, OrderStatus.Cancelled));
 
        if (Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.OrderAlreadyCompleted());
 
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Touch();
        Raise(new OrderCancelledEvent(Id, CustomerId, reason));
    }
 
    private void GuardModifiable()
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new DomainException(
                OrderErrors.InvalidOrderStatusForModification(Status));
    }
}