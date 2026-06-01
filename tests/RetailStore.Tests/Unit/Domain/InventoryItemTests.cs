using FluentAssertions;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Tests.Unit.Domain;

public class InventoryItemTests
{
    private static readonly Guid ValidProductId = Guid.NewGuid();

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsPropertiesCorrectly()
    {
        var item = InventoryItem.Create(ValidProductId, 50, 10);

        item.ProductId.Should().Be(ValidProductId);
        item.QuantityOnHand.Should().Be(50);
        item.ReservedQuantity.Should().Be(0);
        item.ReorderThreshold.Should().Be(10);
        item.AvailableQuantity.Should().Be(50);
    }

    [Fact]
    public void Create_WithEmptyProductId_ThrowsDomainException()
    {
        var act = () => InventoryItem.Create(Guid.Empty, 10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeInitialQuantity_ThrowsDomainException()
    {
        var act = () => InventoryItem.Create(ValidProductId, -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeReorderThreshold_ThrowsDomainException()
    {
        var act = () => InventoryItem.Create(ValidProductId, 10, -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_RaisesInventoryItemCreatedEvent()
    {
        var item = InventoryItem.Create(ValidProductId, 50);

        item.DomainEvents.Should().ContainSingle(e => e is InventoryItemCreatedEvent);
    }

    // ── StockStatus Computed Properties ───────────────────────────────────────

    [Fact]
    public void StockStatus_WhenAvailableQuantityIsZero_IsOutOfStock()
    {
        var item = InventoryItem.Create(ValidProductId, 5, 10);
        item.Reserve(5); // available = 0

        item.IsOutOfStock.Should().BeTrue();
        item.StockStatus.Should().Be("OutOfStock");
    }

    [Fact]
    public void StockStatus_WhenOnHandAtOrBelowThreshold_IsLowStock()
    {
        var item = InventoryItem.Create(ValidProductId, 10, 10); // onHand == threshold

        item.IsLowStock.Should().BeTrue();
        item.IsOutOfStock.Should().BeFalse();
        item.StockStatus.Should().Be("LowStock");
    }

    [Fact]
    public void StockStatus_WhenAboveThreshold_IsInStock()
    {
        var item = InventoryItem.Create(ValidProductId, 50, 10);

        item.IsLowStock.Should().BeFalse();
        item.IsOutOfStock.Should().BeFalse();
        item.StockStatus.Should().Be("InStock");
    }

    // ── AddStock ──────────────────────────────────────────────────────────────

    [Fact]
    public void AddStock_PositiveQuantity_IncreasesQuantityOnHand()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        item.AddStock(10);

        item.QuantityOnHand.Should().Be(30);
    }

    [Fact]
    public void AddStock_ZeroQuantity_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        var act = () => item.AddStock(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddStock_WhenRecoveringFromLowStock_RaisesStockRecoveredEvent()
    {
        var item = InventoryItem.Create(ValidProductId, 5, 10); // starts low
        item.ClearDomainEvents();

        item.AddStock(20); // now 25 > threshold 10

        item.DomainEvents.Should().Contain(e => e is StockRecoveredEvent);
    }

    [Fact]
    public void AddStock_RaisesStockAddedEvent()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.ClearDomainEvents();

        item.AddStock(5);

        item.DomainEvents.Should().Contain(e => e is StockAddedEvent);
    }

    // ── RemoveStock ───────────────────────────────────────────────────────────

    [Fact]
    public void RemoveStock_ValidQuantity_DecreasesQuantityOnHand()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        item.RemoveStock(5);

        item.QuantityOnHand.Should().Be(15);
    }

    [Fact]
    public void RemoveStock_MoreThanOnHand_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 5);

        var act = () => item.RemoveStock(10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveStock_ZeroQuantity_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        var act = () => item.RemoveStock(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveStock_DropsToThreshold_RaisesLowStockAlertEvent()
    {
        var item = InventoryItem.Create(ValidProductId, 15, 10);
        item.ClearDomainEvents();

        item.RemoveStock(5); // now 10 == threshold → LowStock

        item.DomainEvents.Should().Contain(e => e is LowStockAlertEvent);
    }

    // ── Reserve ───────────────────────────────────────────────────────────────

    [Fact]
    public void Reserve_ValidQuantity_IncreasesReservedAndDecreasesAvailable()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        item.Reserve(8);

        item.ReservedQuantity.Should().Be(8);
        item.AvailableQuantity.Should().Be(12);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 5);

        var act = () => item.Reserve(10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reserve_ZeroQuantity_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        var act = () => item.Reserve(0);

        act.Should().Throw<DomainException>();
    }

    // ── ReleaseReservation ────────────────────────────────────────────────────

    [Fact]
    public void ReleaseReservation_PartialAmount_DecreasesReservedQuantity()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(10);

        item.ReleaseReservation(4);

        item.ReservedQuantity.Should().Be(6);
        item.AvailableQuantity.Should().Be(14);
    }

    [Fact]
    public void ReleaseReservation_MoreThanReserved_ClampsToZero()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(5);

        item.ReleaseReservation(100); // release more than reserved

        item.ReservedQuantity.Should().Be(0);
    }

    [Fact]
    public void ReleaseReservation_ZeroQuantity_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(5);

        var act = () => item.ReleaseReservation(0);

        act.Should().Throw<DomainException>();
    }

    // ── FulfillReservation ────────────────────────────────────────────────────

    [Fact]
    public void FulfillReservation_ValidQuantity_DecreasesReservedAndOnHand()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(10);

        item.FulfillReservation(10);

        item.ReservedQuantity.Should().Be(0);
        item.QuantityOnHand.Should().Be(10);
    }

    [Fact]
    public void FulfillReservation_MoreThanReserved_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(5);

        var act = () => item.FulfillReservation(10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FulfillReservation_ZeroQuantity_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(5);

        var act = () => item.FulfillReservation(0);

        act.Should().Throw<DomainException>();
    }

    // ── AdjustQuantity ────────────────────────────────────────────────────────

    [Fact]
    public void AdjustQuantity_ValidNewQuantity_SetsQuantityOnHand()
    {
        var item = InventoryItem.Create(ValidProductId, 50);

        item.AdjustQuantity(30, "inventory count");

        item.QuantityOnHand.Should().Be(30);
    }

    [Fact]
    public void AdjustQuantity_BelowExistingReserved_ClampsReservedToNewQuantity()
    {
        var item = InventoryItem.Create(ValidProductId, 20);
        item.Reserve(15); // reserved = 15

        item.AdjustQuantity(5, "shrinkage"); // new qty = 5, reserved clamped to 5

        item.QuantityOnHand.Should().Be(5);
        item.ReservedQuantity.Should().Be(5);
    }

    [Fact]
    public void AdjustQuantity_NegativeValue_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 20);

        var act = () => item.AdjustQuantity(-1, "error");

        act.Should().Throw<DomainException>();
    }

    // ── UpdateReorderThreshold ────────────────────────────────────────────────

    [Fact]
    public void UpdateReorderThreshold_ValidValue_SetsThreshold()
    {
        var item = InventoryItem.Create(ValidProductId, 50, 10);

        item.UpdateReorderThreshold(20);

        item.ReorderThreshold.Should().Be(20);
    }

    [Fact]
    public void UpdateReorderThreshold_NegativeValue_ThrowsDomainException()
    {
        var item = InventoryItem.Create(ValidProductId, 50, 10);

        var act = () => item.UpdateReorderThreshold(-1);

        act.Should().Throw<DomainException>();
    }
}
