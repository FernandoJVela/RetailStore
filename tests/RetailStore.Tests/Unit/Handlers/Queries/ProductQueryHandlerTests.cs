using FluentAssertions;
using RetailStore.Api.Features.Products.Application.Queries;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Unit.Handlers.Queries;

public class ProductQueryHandlerTests
{
    private static readonly Money Price = new(29.99m, "USD");

    // ── GetProductsHandler ────────────────────────────────────────────────────

    public class GetProductsHandlerTests
    {
        [Fact]
        public async Task Handle_NoProducts_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetProductsHandler(db);

            var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_MultipleProducts_ReturnsAll()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(new ProductBuilder().WithSku("SKU-A").Build());
            db.Add(new ProductBuilder().WithSku("SKU-B").Build());
            db.SaveChanges();

            var result = await new GetProductsHandler(db).Handle(new GetProductsQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_FilterByCategory_ReturnsOnlyMatchingProducts()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(new ProductBuilder().WithSku("E-001").WithCategory("Electronics").Build());
            db.Add(new ProductBuilder().WithSku("C-001").WithCategory("Clothing").Build());
            db.SaveChanges();

            var result = await new GetProductsHandler(db)
                .Handle(new GetProductsQuery(Category: "Electronics"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Category.Should().Be("Electronics");
        }

        [Fact]
        public async Task Handle_FilterByIsActive_ReturnsOnlyActiveProducts()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(new ProductBuilder().WithSku("A-001").Build());          // active
            db.Add(new ProductBuilder().WithSku("I-001").BuildInactive());  // inactive
            db.SaveChanges();

            var result = await new GetProductsHandler(db)
                .Handle(new GetProductsQuery(IsActive: true), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SearchByName_ReturnsMatchingProducts()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Product.Create("Super Widget", "SW-001", Price, "General"));
            db.Add(Product.Create("Standard Gadget", "SG-001", Price, "General"));
            db.SaveChanges();

            var result = await new GetProductsHandler(db)
                .Handle(new GetProductsQuery(Search: "widget"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Name.Should().Be("Super Widget");
        }

        [Fact]
        public async Task Handle_SearchBySku_ReturnsMatchingProducts()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Product.Create("Widget Alpha", "WGT-ALPHA", Price, "General"));
            db.Add(Product.Create("Widget Beta", "WGT-BETA", Price, "General"));
            db.SaveChanges();

            var result = await new GetProductsHandler(db)
                .Handle(new GetProductsQuery(Search: "alpha"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Sku.Should().Be("WGT-ALPHA");
        }

        [Fact]
        public async Task Handle_ValidProduct_ReturnsDtoWithCorrectFields()
        {
            using var db = InMemoryDbContextFactory.Create();
            var product = Product.Create("Widget Pro", "WGT-001", new Money(49.99m, "USD"), "Electronics");
            db.Add(product);
            db.SaveChanges();

            var result = await new GetProductsHandler(db).Handle(new GetProductsQuery(), CancellationToken.None);

            var dto = result.Single();
            dto.Id.Should().Be(product.Id);
            dto.Name.Should().Be("Widget Pro");
            dto.Sku.Should().Be("WGT-001");
            dto.Price.Should().Be(49.99m);
            dto.Currency.Should().Be("USD");
            dto.Category.Should().Be("Electronics");
            dto.IsActive.Should().BeTrue();
        }
    }

    // ── GetProductByIdHandler ─────────────────────────────────────────────────

    public class GetProductByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingProduct_ReturnsProductDetail()
        {
            using var db = InMemoryDbContextFactory.Create();
            var product = Product.Create("Widget Pro", "WGT-001", Price, "Electronics", "A great widget");
            db.Add(product);
            db.SaveChanges();

            var handler = new GetProductByIdHandler(db);
            var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

            result.Id.Should().Be(product.Id);
            result.Name.Should().Be("Widget Pro");
            result.Description.Should().Be("A great widget");
            result.Price.Should().Be(29.99m);
            result.Currency.Should().Be("USD");
        }

        [Fact]
        public async Task Handle_NonExistentProduct_ThrowsDomainException()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetProductByIdHandler(db);

            var act = () => handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ── GetProductsByCategoryHandler ──────────────────────────────────────────

    public class GetProductsByCategoryHandlerTests
    {
        [Fact]
        public async Task Handle_CategoryWithActiveProducts_ReturnsOnlyActiveinCategory()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(Product.Create("Active Widget", "AW-001", Price, "Electronics"));
            var inactive = Product.Create("Inactive Widget", "IW-001", Price, "Electronics");
            inactive.Deactivate();
            db.Add(inactive);
            db.Add(Product.Create("Different Category", "DC-001", Price, "Clothing"));
            db.SaveChanges();

            var handler = new GetProductsByCategoryHandler(db);
            var result = await handler.Handle(new GetProductsByCategoryQuery("Electronics"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Name.Should().Be("Active Widget");
        }

        [Fact]
        public async Task Handle_EmptyCategory_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetProductsByCategoryHandler(db);

            var result = await handler.Handle(new GetProductsByCategoryQuery("NonExistent"), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
