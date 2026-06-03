using FluentAssertions;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.Unit.Domain;

public class OrderTests
{
    private static readonly Money ValidPrice = new(10m, "USD");

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidCustomerId_ReturnsOrderInDraftStatus()
    {
        var customerId = Guid.NewGuid();

        var order = Order.Create(customerId);

        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Draft);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithFutureOrderDate_ThrowsDomainException()
    {
        var futureDate = DateTime.UtcNow.AddMinutes(10);

        var act = () => Order.Create(Guid.NewGuid(), futureDate);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_RaisesOrderCreatedEvent()
    {
        var order = Order.Create(Guid.NewGuid());

        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedEvent);
    }

    // ── AddItem ───────────────────────────────────────────────────────────────

    [Fact]
    public void AddItem_NewProduct_AppearsInItemsCollection()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();

        order.AddItem(productId, 2, ValidPrice);

        order.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);
    }

    [Fact]
    public void AddItem_SameProductTwice_IncrementsQuantityInsteadOfAddingLine()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();

        order.AddItem(productId, 2, ValidPrice);
        order.AddItem(productId, 3, ValidPrice);

        order.Items.Should().ContainSingle();
        order.Items.Single().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_WithZeroPrice_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());

        var act = () => order.AddItem(Guid.NewGuid(), 1, new Money(0m, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_ToCompletedOrder_ThrowsDomainException()
    {
        var order = BuildDeliveredOrderWithItem();
        order.Complete();

        var act = () => order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_ToCancelledOrder_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        order.Cancel("test");

        var act = () => order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_RaisesOrderItemAddedEvent()
    {
        var order = Order.Create(Guid.NewGuid());
        order.ClearDomainEvents();

        order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        order.DomainEvents.Should().ContainSingle(e => e is OrderItemAddedEvent);
    }

    // ── RemoveItem ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesFromItemsCollection()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, ValidPrice);

        order.RemoveItem(productId);

        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());

        var act = () => order.RemoveItem(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveItem_RaisesOrderItemRemovedEvent()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, ValidPrice);
        order.ClearDomainEvents();

        order.RemoveItem(productId);

        order.DomainEvents.Should().ContainSingle(e => e is OrderItemRemovedEvent);
    }

    // ── UpdateItemQuantity ────────────────────────────────────────────────────

    [Fact]
    public void UpdateItemQuantity_ValidQuantity_UpdatesItem()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, ValidPrice);

        order.UpdateItemQuantity(productId, 5);

        order.Items.Single().Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_ZeroOrNegative_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, ValidPrice);

        var act = () => order.UpdateItemQuantity(productId, 0);

        act.Should().Throw<DomainException>();
    }

    // ── TotalAmount ───────────────────────────────────────────────────────────

    [Fact]
    public void TotalAmount_WithMultipleItems_ReturnsSumOfSubtotals()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 2, new Money(10m, "USD")); // 20
        order.AddItem(Guid.NewGuid(), 3, new Money(5m, "USD"));  // 15

        order.TotalAmount.Should().Be(35m);
    }

    // ── Confirm ───────────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_DraftOrderWithItems_StatusBecomesConfirmed()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_EmptyOrder_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());

        var act = () => order.Confirm();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_ThrowsDomainException()
    {
        var order = BuildConfirmedOrderWithItem();

        var act = () => order.Confirm();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_RaisesOrderConfirmedEvent()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);
        order.ClearDomainEvents();

        order.Confirm();

        order.DomainEvents.Should().ContainSingle(e => e is OrderConfirmedEvent);
    }

    // ── MarkShipped ───────────────────────────────────────────────────────────

    [Fact]
    public void MarkShipped_ConfirmedOrder_StatusBecomesShipped()
    {
        var order = BuildConfirmedOrderWithItem();

        order.MarkShipped();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void MarkShipped_DraftOrder_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        var act = () => order.MarkShipped();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkShipped_AlreadyShippedOrder_ThrowsDomainException()
    {
        var order = BuildConfirmedOrderWithItem();
        order.MarkShipped();

        var act = () => order.MarkShipped();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkShipped_RaisesOrderShippedEvent()
    {
        var order = BuildConfirmedOrderWithItem();
        order.ClearDomainEvents();

        order.MarkShipped();

        order.DomainEvents.Should().ContainSingle(e => e is OrderShippedEvent);
    }

    // ── MarkDelivered ─────────────────────────────────────────────────────────

    [Fact]
    public void MarkDelivered_ShippedOrder_StatusBecomesDelivered()
    {
        var order = BuildConfirmedOrderWithItem();
        order.MarkShipped();

        order.MarkDelivered();

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void MarkDelivered_ConfirmedOrder_ThrowsDomainException()
    {
        var order = BuildConfirmedOrderWithItem();

        var act = () => order.MarkDelivered();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkDelivered_AlreadyDeliveredOrder_ThrowsDomainException()
    {
        var order = BuildDeliveredOrderWithItem();

        var act = () => order.MarkDelivered();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkDelivered_RaisesOrderDeliveredEvent()
    {
        var order = BuildConfirmedOrderWithItem();
        order.MarkShipped();
        order.ClearDomainEvents();

        order.MarkDelivered();

        order.DomainEvents.Should().ContainSingle(e => e is OrderDeliveredEvent);
    }

    // ── Complete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_DeliveredOrder_StatusBecomesCompletedAndCompletedAtIsSet()
    {
        var order = BuildDeliveredOrderWithItem();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ConfirmedOrder_ThrowsDomainException()
    {
        var order = BuildConfirmedOrderWithItem();

        var act = () => order.Complete();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_DraftOrder_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        var act = () => order.Complete();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_RaisesOrderCompletedEvent()
    {
        var order = BuildDeliveredOrderWithItem();
        order.ClearDomainEvents();

        order.Complete();

        order.DomainEvents.Should().ContainSingle(e => e is OrderCompletedEvent);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_DraftOrder_StatusBecomesCancelledAndCancelledAtIsSet()
    {
        var order = Order.Create(Guid.NewGuid());

        order.Cancel("customer requested");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_CompletedOrder_ThrowsDomainException()
    {
        var order = BuildDeliveredOrderWithItem();
        order.Complete();

        var act = () => order.Cancel("too late");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        order.Cancel("first cancel");

        var act = () => order.Cancel("second cancel");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_RaisesOrderCancelledEvent()
    {
        var order = Order.Create(Guid.NewGuid());
        order.ClearDomainEvents();

        order.Cancel("reason");

        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
    }

    // ── Full lifecycle ────────────────────────────────────────────────────────

    [Fact]
    public void FullLifecycle_Draft_Confirmed_Shipped_Delivered_Completed()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);

        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);

        order.MarkShipped();
        order.Status.Should().Be(OrderStatus.Shipped);

        order.MarkDelivered();
        order.Status.Should().Be(OrderStatus.Delivered);

        order.Complete();
        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Order BuildConfirmedOrderWithItem()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, ValidPrice);
        order.Confirm();
        return order;
    }

    private static Order BuildDeliveredOrderWithItem()
    {
        var order = BuildConfirmedOrderWithItem();
        order.MarkShipped();
        order.MarkDelivered();
        return order;
    }
}
