using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Inventory.Infrastructure;
using RetailStore.Tests.TestHelpers;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Repositories;

/// <summary>
/// Verifies InventoryRepository against a real SQLite database.
/// Key behaviors: GetByProductIdAsync (FK-based lookup), ExistsForProductAsync,
/// and stock mutations persisting correctly.
/// </summary>
public class InventoryRepositoryTests
{
    private static (InventoryRepository repo, RetailStore.Infrastructure.Persistence.RetailStoreDbContext db)
        Create()
    {
        var db = InMemoryDbContextFactory.Create();
        return (new InventoryRepository(db), db);
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewItem_PersistsToDatabase()
    {
        var (repo, db) = Create();
        var productId = Guid.NewGuid();
        var item = new InventoryItemBuilder().ForProduct(productId).WithQuantity(50).Build();

        await repo.AddAsync(item);
        db.SaveChanges();

        var persisted = await db.Set<InventoryItem>().AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == item.Id);
        persisted.Should().NotBeNull();
        persisted!.ProductId.Should().Be(productId);
        persisted.QuantityOnHand.Should().Be(50);
        persisted.ReservedQuantity.Should().Be(0);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        var (repo, db) = Create();
        var item = new InventoryItemBuilder().Build();
        await repo.AddAsync(item);
        db.SaveChanges();

        var result = await repo.GetByIdAsync(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetByProductIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByProductIdAsync_ExistingProduct_ReturnsCorrectItem()
    {
        var (repo, db) = Create();
        var productId = Guid.NewGuid();
        var item = new InventoryItemBuilder().ForProduct(productId).WithQuantity(30).Build();
        await repo.AddAsync(item);
        db.SaveChanges();

        var result = await repo.GetByProductIdAsync(productId);

        result.Should().NotBeNull();
        result!.ProductId.Should().Be(productId);
        result.QuantityOnHand.Should().Be(30);
    }

    [Fact]
    public async Task GetByProductIdAsync_NonExistentProduct_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByProductIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProductIdAsync_MultipleItemsInDb_ReturnsOnlyMatchingProduct()
    {
        var (repo, db) = Create();
        var targetId = Guid.NewGuid();
        await repo.AddAsync(new InventoryItemBuilder().ForProduct(Guid.NewGuid()).Build());
        await repo.AddAsync(new InventoryItemBuilder().ForProduct(targetId).WithQuantity(99).Build());
        await repo.AddAsync(new InventoryItemBuilder().ForProduct(Guid.NewGuid()).Build());
        db.SaveChanges();

        var result = await repo.GetByProductIdAsync(targetId);

        result.Should().NotBeNull();
        result!.QuantityOnHand.Should().Be(99);
    }

    // ── ExistsForProductAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ExistsForProductAsync_ExistingProduct_ReturnsTrue()
    {
        var (repo, db) = Create();
        var productId = Guid.NewGuid();
        var item = new InventoryItemBuilder().ForProduct(productId).Build();
        await repo.AddAsync(item);
        db.SaveChanges();

        var result = await repo.ExistsForProductAsync(productId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsForProductAsync_NonExistentProduct_ReturnsFalse()
    {
        var (repo, _) = Create();

        var result = await repo.ExistsForProductAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_MultipleItems_ReturnsAll()
    {
        var (repo, db) = Create();
        await repo.AddAsync(new InventoryItemBuilder().ForProduct(Guid.NewGuid()).Build());
        await repo.AddAsync(new InventoryItemBuilder().ForProduct(Guid.NewGuid()).Build());
        db.SaveChanges();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_AfterAddingStock_PersistsNewQuantityOnHand()
    {
        var (repo, db) = Create();
        var item = new InventoryItemBuilder().WithQuantity(20).Build();
        await repo.AddAsync(item);
        db.SaveChanges();

        item.AddStock(15);
        repo.Update(item);
        db.SaveChanges();

        db.ChangeTracker.Clear();
        var fresh = await db.Set<InventoryItem>().AsNoTracking()
            .FirstAsync(i => i.Id == item.Id);
        fresh.QuantityOnHand.Should().Be(35);
    }

    [Fact]
    public async Task Update_AfterReservation_PersistsReservedQuantity()
    {
        var (repo, db) = Create();
        var item = new InventoryItemBuilder().WithQuantity(20).Build();
        await repo.AddAsync(item);
        db.SaveChanges();

        item.Reserve(8);
        repo.Update(item);
        db.SaveChanges();

        db.ChangeTracker.Clear();
        var fresh = await db.Set<InventoryItem>().AsNoTracking()
            .FirstAsync(i => i.Id == item.Id);
        fresh.ReservedQuantity.Should().Be(8);
        fresh.AvailableQuantity.Should().Be(12);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_ExistingItem_DeletesFromDatabase()
    {
        var (repo, db) = Create();
        var item = new InventoryItemBuilder().Build();
        await repo.AddAsync(item);
        db.SaveChanges();
        var id = item.Id;

        repo.Remove(item);
        db.SaveChanges();

        var result = await db.Set<InventoryItem>().AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
        result.Should().BeNull();
    }
}
