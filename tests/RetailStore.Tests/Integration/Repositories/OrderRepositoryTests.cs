using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Orders.Infrastructure;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Repositories;

/// <summary>
/// Verifies OrderRepository against a real SQLite database.
/// Key behaviors: GetByIdAsync includes Items (both AutoInclude config + explicit Include),
/// and cascading delete removes order items when the order is removed.
/// </summary>
public class OrderRepositoryTests
{
    private static (OrderRepository repo, RetailStore.Infrastructure.Persistence.RetailStoreDbContext db)
        Create()
    {
        var db = InMemoryDbContextFactory.Create();
        return (new OrderRepository(db), db);
    }

    private static Order MakeOrder(int itemCount = 1)
    {
        var order = Order.Create(Guid.NewGuid());
        for (var i = 0; i < itemCount; i++)
            order.AddItem(Guid.NewGuid(), quantity: i + 1, new Money(10m, "USD"));
        return order;
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewOrder_PersistsToDatabase()
    {
        var (repo, db) = Create();
        var order = MakeOrder();

        await repo.AddAsync(order);
        db.SaveChanges();

        var persisted = await db.Set<Order>().AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(order.CustomerId);
        persisted.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public async Task AddAsync_OrderWithItems_PersistsItemsAlongWithOrder()
    {
        var (repo, db) = Create();
        var order = MakeOrder(itemCount: 3);

        await repo.AddAsync(order);
        db.SaveChanges();

        var items = await db.Set<OrderItem>()
            .AsNoTracking()
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();

        items.Should().HaveCount(3);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsOrder()
    {
        var (repo, db) = Create();
        var order = MakeOrder();
        await repo.AddAsync(order);
        db.SaveChanges();

        var result = await repo.GetByIdAsync(order.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsOrderItemsEagerly()
    {
        var (repo, db) = Create();
        var productId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid());
        order.AddItem(productId, quantity: 5, new Money(15m, "USD"));
        await repo.AddAsync(order);
        db.SaveChanges();

        // Clear the tracker so GetByIdAsync can't short-circuit via identity map
        db.ChangeTracker.Clear();

        var result = await repo.GetByIdAsync(order.Id);

        result!.Items.Should().ContainSingle();
        result.Items.Single().ProductId.Should().Be(productId);
        result.Items.Single().Quantity.Should().Be(5);
        result.Items.Single().UnitPrice.Should().Be(15m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_MultipleOrders_ReturnsAllOrders()
    {
        var (repo, db) = Create();
        await repo.AddAsync(MakeOrder());
        await repo.AddAsync(MakeOrder());
        db.SaveChanges();

        var result = await repo.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ConfirmedOrder_PersistsStatusChange()
    {
        var (repo, db) = Create();
        var order = new OrderBuilder().BuildWithItem();
        await repo.AddAsync(order);
        db.SaveChanges();

        order.Confirm();
        repo.Update(order);
        db.SaveChanges();

        db.ChangeTracker.Clear();
        var fresh = await db.Set<Order>().AsNoTracking()
            .FirstAsync(o => o.Id == order.Id);
        fresh.Status.Should().Be(OrderStatus.Confirmed);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_Order_DeletesOrderAndCascadesItems()
    {
        var (repo, db) = Create();
        var order = MakeOrder(itemCount: 2);
        await repo.AddAsync(order);
        db.SaveChanges();
        var orderId = order.Id;

        repo.Remove(order);
        db.SaveChanges();

        var orderResult = await db.Set<Order>().AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
        orderResult.Should().BeNull();

        var itemsResult = await db.Set<OrderItem>().AsNoTracking()
            .Where(i => i.OrderId == orderId).ToListAsync();
        itemsResult.Should().BeEmpty();
    }
}
