using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Products.Infrastructure;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Repositories;

/// <summary>
/// Verifies ProductRepository against a real SQLite database.
/// Each test uses its own isolated DbContext. AsNoTracking() queries are used
/// for post-save assertions to bypass EF Core's identity map and force a real DB read.
/// </summary>
public class ProductRepositoryTests
{
    private static (ProductRepository repo, RetailStore.Infrastructure.Persistence.RetailStoreDbContext db)
        Create()
    {
        var db = InMemoryDbContextFactory.Create();
        return (new ProductRepository(db), db);
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidProduct_PersistsToDatabase()
    {
        var (repo, db) = Create();
        var product = new ProductBuilder().WithName("Widget Pro").WithSku("WGT-001").Build();

        await repo.AddAsync(product);
        db.SaveChanges();

        var persisted = await db.Set<Product>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Widget Pro");
        persisted.Sku.Should().Be("WGT-001");
    }

    [Fact]
    public async Task AddAsync_Product_PreservesMoneyComplexProperty()
    {
        var (repo, db) = Create();
        var product = Product.Create("Widget", "WGT-002", new Money(49.99m, "EUR"), "Electronics");

        await repo.AddAsync(product);
        db.SaveChanges();

        var persisted = await db.Set<Product>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        persisted!.Price.Amount.Should().Be(49.99m);
        persisted.Price.Currency.Should().Be("EUR");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        var (repo, db) = Create();
        var product = new ProductBuilder().WithSku("GET-001").Build();
        await repo.AddAsync(product);
        db.SaveChanges();

        var result = await repo.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
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
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var (repo, _) = Create();

        var result = await repo.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ThreeProducts_ReturnsAll()
    {
        var (repo, db) = Create();
        await repo.AddAsync(new ProductBuilder().WithSku("A-001").Build());
        await repo.AddAsync(new ProductBuilder().WithSku("A-002").Build());
        await repo.AddAsync(new ProductBuilder().WithSku("A-003").Build());
        db.SaveChanges();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(3);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ChangedPrice_PersistsNewPriceToDatabase()
    {
        var (repo, db) = Create();
        var product = new ProductBuilder().WithSku("UPD-001").WithPrice(10m).Build();
        await repo.AddAsync(product);
        db.SaveChanges();

        product.UpdatePrice(new Money(99.99m, "USD"));
        repo.Update(product);
        db.SaveChanges();

        var fresh = await db.Set<Product>().AsNoTracking()
            .FirstAsync(p => p.Id == product.Id);
        fresh.Price.Amount.Should().Be(99.99m);
    }

    [Fact]
    public async Task Update_DeactivatedProduct_PersistsIsActiveFalse()
    {
        var (repo, db) = Create();
        var product = new ProductBuilder().WithSku("DEA-001").Build();
        await repo.AddAsync(product);
        db.SaveChanges();

        product.Deactivate();
        repo.Update(product);
        db.SaveChanges();

        var fresh = await db.Set<Product>().AsNoTracking()
            .FirstAsync(p => p.Id == product.Id);
        fresh.IsActive.Should().BeFalse();
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_ExistingProduct_DeletesFromDatabase()
    {
        var (repo, db) = Create();
        var product = new ProductBuilder().WithSku("REM-001").Build();
        await repo.AddAsync(product);
        db.SaveChanges();

        repo.Remove(product);
        db.SaveChanges();

        var result = await db.Set<Product>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        result.Should().BeNull();
    }
}
