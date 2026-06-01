using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Api.Features.Customers.Infrastructure;
using RetailStore.Tests.TestHelpers;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Repositories;

/// <summary>
/// Verifies CustomerRepository against a real SQLite database.
/// Key behaviors: email normalization in GetByEmailAsync/ExistsWithEmailAsync,
/// unique email index enforcement, and full CRUD.
/// </summary>
public class CustomerRepositoryTests
{
    private static (CustomerRepository repo, RetailStore.Infrastructure.Persistence.RetailStoreDbContext db)
        Create()
    {
        var db = InMemoryDbContextFactory.Create();
        return (new CustomerRepository(db), db);
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidCustomer_PersistsToDatabase()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder()
            .WithFirstName("Alice")
            .WithLastName("Wonder")
            .WithEmail("alice@example.com")
            .Build();

        await repo.AddAsync(customer);
        db.SaveChanges();

        var persisted = await db.Set<Customer>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customer.Id);
        persisted.Should().NotBeNull();
        persisted!.FirstName.Should().Be("Alice");
        persisted.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task AddAsync_CustomerWithShippingAddress_PersistsFlatColumns()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder()
            .WithEmail("bob@example.com")
            .WithAddress("123 Main St", "Springfield", "IL", "62701", "US")
            .Build();

        await repo.AddAsync(customer);
        db.SaveChanges();

        var persisted = await db.Set<Customer>().AsNoTracking()
            .FirstAsync(c => c.Id == customer.Id);
        persisted.ShippingStreet.Should().Be("123 Main St");
        persisted.ShippingCity.Should().Be("Springfield");
        persisted.ShippingCountry.Should().Be("US");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomer()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("cid@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        var result = await repo.GetByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetByEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_ExactMatch_ReturnsCustomer()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("find.me@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        var result = await repo.GetByEmailAsync("find.me@example.com");

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_UppercaseQuery_StillFindsLowercaseStoredEmail()
    {
        var (repo, db) = Create();
        // Domain normalizes to lowercase on Register; repository also normalizes the query
        var customer = new CustomerBuilder().WithEmail("case.test@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        var result = await repo.GetByEmailAsync("CASE.TEST@EXAMPLE.COM");

        result.Should().NotBeNull();
        result!.Email.Should().Be("case.test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByEmailAsync("nobody@example.com");

        result.Should().BeNull();
    }

    // ── ExistsWithEmailAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ExistsWithEmailAsync_ExistingEmail_ReturnsTrue()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("exists@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        var result = await repo.ExistsWithEmailAsync("exists@example.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_NonExistentEmail_ReturnsFalse()
    {
        var (repo, _) = Create();

        var result = await repo.ExistsWithEmailAsync("ghost@example.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_IsCaseInsensitive()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("mixed@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        var result = await repo.ExistsWithEmailAsync("MIXED@EXAMPLE.COM");

        result.Should().BeTrue();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_MultipleCustomers_ReturnsAll()
    {
        var (repo, db) = Create();
        await repo.AddAsync(new CustomerBuilder().WithEmail("a@example.com").Build());
        await repo.AddAsync(new CustomerBuilder().WithEmail("b@example.com").Build());
        await repo.AddAsync(new CustomerBuilder().WithEmail("c@example.com").Build());
        db.SaveChanges();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(3);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_NameChange_PersistsToDatabase()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("upd@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        customer.UpdateName("Updated", "Name");
        repo.Update(customer);
        db.SaveChanges();

        db.ChangeTracker.Clear();
        var fresh = await db.Set<Customer>().AsNoTracking()
            .FirstAsync(c => c.Id == customer.Id);
        fresh.FirstName.Should().Be("Updated");
        fresh.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task Update_DeactivatedCustomer_PersistsIsActiveFalse()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("dea@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();

        customer.Deactivate();
        repo.Update(customer);
        db.SaveChanges();

        db.ChangeTracker.Clear();
        var fresh = await db.Set<Customer>().AsNoTracking()
            .FirstAsync(c => c.Id == customer.Id);
        fresh.IsActive.Should().BeFalse();
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_ExistingCustomer_DeletesFromDatabase()
    {
        var (repo, db) = Create();
        var customer = new CustomerBuilder().WithEmail("del@example.com").Build();
        await repo.AddAsync(customer);
        db.SaveChanges();
        var id = customer.Id;

        repo.Remove(customer);
        db.SaveChanges();

        var result = await db.Set<Customer>().AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        result.Should().BeNull();
    }

    // ── Unique email index enforcement ────────────────────────────────────────

    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsOnSaveChanges()
    {
        var (repo, db) = Create();
        var email = "dup@example.com";
        await repo.AddAsync(new CustomerBuilder().WithEmail(email).Build());
        db.SaveChanges();

        await repo.AddAsync(new CustomerBuilder().WithEmail(email).Build());
        var act = () => db.SaveChanges();

        act.Should().Throw<Exception>(); // SQLite unique constraint violation
    }
}
