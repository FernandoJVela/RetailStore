using FluentAssertions;
using RetailStore.Api.Features.Customers.Application.Commands;
using RetailStore.Api.Features.Customers.Application.Queries;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;

namespace RetailStore.Tests.Unit.Handlers.Queries;

public class CustomerQueryHandlerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Customer Make(
        string first = "John", string last = "Doe",
        string email = "john@example.com", bool active = true)
    {
        var c = Customer.Register(first, last, email);
        if (!active) c.Deactivate();
        return c;
    }

    // ── GetCustomersHandler ───────────────────────────────────────────────────

    public class GetCustomersHandlerTests
    {
        [Fact]
        public async Task Handle_NoCustomers_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var result = await new GetCustomersHandler(db).Handle(new GetCustomersQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_MultipleCustomers_ReturnsAll()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(email: "alice@example.com"));
            db.Add(Make(first: "Bob", last: "Smith", email: "bob@example.com"));
            db.SaveChanges();

            var result = await new GetCustomersHandler(db).Handle(new GetCustomersQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_FilterByIsActive_ReturnsOnlyActiveCustomers()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(email: "active@example.com", active: true));
            db.Add(Make(email: "inactive@example.com", active: false));
            db.SaveChanges();

            var result = await new GetCustomersHandler(db)
                .Handle(new GetCustomersQuery(IsActive: true), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SearchByFirstName_ReturnsMatchingCustomers()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(first: "Alice", last: "Adams", email: "alice@example.com"));
            db.Add(Make(first: "Bob", last: "Brown", email: "bob@example.com"));
            db.SaveChanges();

            var result = await new GetCustomersHandler(db)
                .Handle(new GetCustomersQuery(Search: "alice"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].FirstName.Should().Be("Alice");
        }

        [Fact]
        public async Task Handle_SearchByEmail_ReturnsMatchingCustomers()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(email: "unique.user@company.com"));
            db.Add(Make(email: "other@example.com"));
            db.SaveChanges();

            var result = await new GetCustomersHandler(db)
                .Handle(new GetCustomersQuery(Search: "company.com"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Email.Should().Be("unique.user@company.com");
        }

        [Fact]
        public async Task Handle_ValidCustomer_ReturnsDtoWithCorrectFields()
        {
            using var db = InMemoryDbContextFactory.Create();
            var customer = Make(first: "Jane", last: "Smith", email: "jane@example.com");
            db.Add(customer);
            db.SaveChanges();

            var result = await new GetCustomersHandler(db).Handle(new GetCustomersQuery(), CancellationToken.None);

            var dto = result.Single();
            dto.Id.Should().Be(customer.Id);
            dto.FirstName.Should().Be("Jane");
            dto.LastName.Should().Be("Smith");
            dto.FullName.Should().Be("Jane Smith");
            dto.Email.Should().Be("jane@example.com");
            dto.IsActive.Should().BeTrue();
        }
    }

    // ── GetCustomerByIdHandler ────────────────────────────────────────────────

    public class GetCustomerByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingCustomer_ReturnsCustomerDetail()
        {
            using var db = InMemoryDbContextFactory.Create();
            var customer = Make(first: "Alice", last: "Wonder", email: "alice@example.com");
            db.Add(customer);
            db.SaveChanges();

            var handler = new GetCustomerByIdHandler(db);
            var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

            result.Id.Should().Be(customer.Id);
            result.FirstName.Should().Be("Alice");
            result.LastName.Should().Be("Wonder");
            result.FullName.Should().Be("Alice Wonder");
            result.Email.Should().Be("alice@example.com");
        }

        [Fact]
        public async Task Handle_CustomerWithShippingAddress_ReturnsAddressInDto()
        {
            using var db = InMemoryDbContextFactory.Create();
            var customer = Make(email: "bob@example.com");
            customer.UpdateShippingAddress(new Address("123 Main St", "Springfield", "IL", "62701", "US"));
            db.Add(customer);
            db.SaveChanges();

            var handler = new GetCustomerByIdHandler(db);
            var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

            result.ShippingAddress.Should().NotBeNull();
            result.ShippingAddress!.Street.Should().Be("123 Main St");
            result.ShippingAddress.City.Should().Be("Springfield");
            result.ShippingAddress.Country.Should().Be("US");
        }

        [Fact]
        public async Task Handle_CustomerWithoutShippingAddress_ShippingAddressIsNull()
        {
            using var db = InMemoryDbContextFactory.Create();
            var customer = Make(email: "noadr@example.com");
            db.Add(customer);
            db.SaveChanges();

            var handler = new GetCustomerByIdHandler(db);
            var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

            result.ShippingAddress.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NonExistentCustomer_ThrowsDomainException()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetCustomerByIdHandler(db);

            var act = () => handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ── GetCustomerByEmailHandler ─────────────────────────────────────────────

    public class GetCustomerByEmailHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingEmail_ReturnsCustomer()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(email: "find.me@example.com"));
            db.SaveChanges();

            var handler = new GetCustomerByEmailHandler(db);
            var result = await handler.Handle(
                new GetCustomerByEmailQuery("find.me@example.com"), CancellationToken.None);

            result.Email.Should().Be("find.me@example.com");
        }

        [Fact]
        public async Task Handle_EmailLookupIsCaseInsensitive()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Make(email: "case.test@example.com")); // stored lowercase
            db.SaveChanges();

            var handler = new GetCustomerByEmailHandler(db);
            // Query normalizes to lowercase before searching
            var result = await handler.Handle(
                new GetCustomerByEmailQuery("CASE.TEST@EXAMPLE.COM"), CancellationToken.None);

            result.Email.Should().Be("case.test@example.com");
        }

        [Fact]
        public async Task Handle_NonExistentEmail_ThrowsDomainException()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetCustomerByEmailHandler(db);

            var act = () => handler.Handle(
                new GetCustomerByEmailQuery("nobody@example.com"), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
