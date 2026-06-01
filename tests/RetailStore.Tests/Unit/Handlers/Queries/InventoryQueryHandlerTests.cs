using FluentAssertions;
using RetailStore.Api.Features.Inventory.Application.Queries;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;

namespace RetailStore.Tests.Unit.Handlers.Queries;

public class InventoryQueryHandlerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (Product product, InventoryItem item) MakePair(
        string sku = "WGT-001",
        int quantity = 50,
        int threshold = 10)
    {
        var product = Product.Create("Widget", sku, new Money(10m, "USD"), "General");
        var item = InventoryItem.Create(product.Id, quantity, threshold);
        return (product, item);
    }

    // ── GetInventoryHandler ───────────────────────────────────────────────────

    public class GetInventoryHandlerTests
    {
        [Fact]
        public async Task Handle_NoItems_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var result = await new GetInventoryHandler(db).Handle(new GetInventoryQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_SingleItem_ReturnsDtoWithProductNameAndSku()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair("TST-001", quantity: 50, threshold: 10);
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var result = await new GetInventoryHandler(db).Handle(new GetInventoryQuery(), CancellationToken.None);

            var dto = result.Single();
            dto.ProductId.Should().Be(product.Id);
            dto.ProductName.Should().Be("Widget");
            dto.Sku.Should().Be("TST-001");
            dto.QuantityOnHand.Should().Be(50);
            dto.ReservedQuantity.Should().Be(0);
            dto.AvailableQuantity.Should().Be(50);
            dto.ReorderThreshold.Should().Be(10);
            dto.StockStatus.Should().Be("InStock");
        }

        [Fact]
        public async Task Handle_FilterByStockStatus_ReturnsOnlyMatchingItems()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (p1, inStock) = MakePair("IN-001", quantity: 50, threshold: 10);
            var (p2, lowStock) = MakePair("LOW-001", quantity: 5, threshold: 10);
            db.AddRange(p1, p2, inStock, lowStock);
            db.SaveChanges();

            var result = await new GetInventoryHandler(db)
                .Handle(new GetInventoryQuery(StockStatus: "LowStock"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].StockStatus.Should().Be("LowStock");
        }

        [Fact]
        public async Task Handle_ItemWithReservation_ReturnsCorrectAvailableQuantity()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair(quantity: 20);
            item.Reserve(8);
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var result = await new GetInventoryHandler(db).Handle(new GetInventoryQuery(), CancellationToken.None);

            var dto = result.Single();
            dto.QuantityOnHand.Should().Be(20);
            dto.ReservedQuantity.Should().Be(8);
            dto.AvailableQuantity.Should().Be(12);
        }

        [Fact]
        public async Task Handle_OutOfStockItem_ReturnsOutOfStockStatus()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair(quantity: 5, threshold: 10);
            item.Reserve(5); // available = 0
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var result = await new GetInventoryHandler(db).Handle(new GetInventoryQuery(), CancellationToken.None);

            result.Single().StockStatus.Should().Be("OutOfStock");
        }
    }

    // ── GetInventoryByProductHandler ──────────────────────────────────────────

    public class GetInventoryByProductHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingProduct_ReturnsInventoryDetail()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair("DET-001", quantity: 30, threshold: 5);
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var handler = new GetInventoryByProductHandler(db);
            var result = await handler.Handle(new GetInventoryByProductQuery(product.Id), CancellationToken.None);

            result.ProductId.Should().Be(product.Id);
            result.ProductName.Should().Be("Widget");
            result.Sku.Should().Be("DET-001");
            result.QuantityOnHand.Should().Be(30);
            result.ReorderThreshold.Should().Be(5);
            result.StockStatus.Should().Be("InStock");
        }

        [Fact]
        public async Task Handle_NonExistentProduct_ThrowsDomainException()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetInventoryByProductHandler(db);

            var act = () => handler.Handle(new GetInventoryByProductQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ── GetLowStockHandler ────────────────────────────────────────────────────

    public class GetLowStockHandlerTests
    {
        [Fact]
        public async Task Handle_AllItemsInStock_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair(quantity: 50, threshold: 10);
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var result = await new GetLowStockHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ItemAtThreshold_ReturnedAsLowStock()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (product, item) = MakePair(quantity: 10, threshold: 10); // exactly at threshold
            db.Add(product);
            db.Add(item);
            db.SaveChanges();

            var result = await new GetLowStockHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].StockStatus.Should().Be("LowStock");
        }

        [Fact]
        public async Task Handle_MixedStock_ReturnsOnlyLowStockItems()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (p1, inStock) = MakePair("IN-001", quantity: 50, threshold: 10);
            var (p2, low) = MakePair("LO-001", quantity: 3, threshold: 10);
            var (p3, outOf) = MakePair("OO-001", quantity: 0, threshold: 10);
            db.AddRange(p1, p2, p3, inStock, low, outOf);
            db.SaveChanges();

            var result = await new GetLowStockHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

            // Low stock query returns items where QuantityOnHand <= ReorderThreshold (both low and out-of-stock)
            result.Should().HaveCount(2);
            result.Should().OnlyContain(i => i.QuantityOnHand <= i.ReorderThreshold);
        }

        [Fact]
        public async Task Handle_LowStockItems_OrderedByAvailableQuantityAscending()
        {
            using var db = InMemoryDbContextFactory.Create();
            var (p1, critical) = MakePair("CR-001", quantity: 1, threshold: 10);
            var (p2, warning) = MakePair("WR-001", quantity: 8, threshold: 10);
            db.AddRange(p1, p2, critical, warning);
            db.SaveChanges();

            var result = await new GetLowStockHandler(db).Handle(new GetLowStockQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].AvailableQuantity.Should().BeLessThanOrEqualTo(result[1].AvailableQuantity);
        }
    }
}
