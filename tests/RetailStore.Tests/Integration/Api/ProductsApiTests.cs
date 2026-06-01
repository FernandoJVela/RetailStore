using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Api;

/// <summary>
/// HTTP-level tests for ProductsController (api/v1/products).
/// Each assertion exercises the full request pipeline: routing → auth → validation
/// → MediatR handler → EF Core → serialized HTTP response.
/// </summary>
public class ProductsApiTests : IntegrationTestBase, IClassFixture<RetailStoreWebAppFactory>
{
    private const string Base = "/api/v1/products";

    // Unique prefix per test to avoid unique-index collisions inside the shared test DB
    private static string UniqueSku(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    public ProductsApiTests(RetailStoreWebAppFactory factory) : base(factory) { }

    // ── Auth guard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_AnonymousRequest_Returns401()
    {
        var response = await CreateAnonymousClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_AuthenticatedRequest_Returns200()
    {
        var response = await CreateAdminClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── POST /api/v1/products ─────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidProduct_Returns201WithLocationHeader()
    {
        var client = CreateAdminClient();

        var response = await client.PostAsJsonAsync(Base, new
        {
            name = "Widget Pro",
            sku = UniqueSku("WGT"),
            price = 29.99m,
            currency = "USD",
            category = "Electronics"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidProduct_CanBeRetrievedByReturnedId()
    {
        var client = CreateAdminClient();
        var sku = UniqueSku("RET");

        var createResp = await client.PostAsJsonAsync(Base, new
        {
            name = "Retrievable Widget",
            sku,
            price = 15m,
            currency = "USD",
            category = "General"
        });
        createResp.EnsureSuccessStatusCode();

        var id = await createResp.Content.ReadFromJsonAsync<Guid>();
        var getResp = await client.GetAsync($"{Base}/{id}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResp.Content.ReadFromJsonAsync<ProductDetailDto>();
        product!.Name.Should().Be("Retrievable Widget");
        product.Sku.Should().Be(sku.ToUpperInvariant());
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            name = "",
            sku = UniqueSku("INV"),
            price = 10m,
            currency = "USD",
            category = "Electronics"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidCurrencyLength_Returns400()
    {
        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            name = "Widget",
            sku = UniqueSku("CUR"),
            price = 10m,
            currency = "US",   // only 2 chars — must be exactly 3
            category = "Electronics"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/v1/products/{id} ─────────────────────────────────────────────

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var response = await CreateAdminClient().GetAsync($"{Base}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsDtoWithCorrectFields()
    {
        var product = new ProductBuilder()
            .WithName("Detail Widget")
            .WithSku(UniqueSku("DW"))
            .WithPrice(49.99m)
            .WithCategory("Electronics")
            .Build();
        Seed(product);

        var response = await CreateAdminClient().GetAsync($"{Base}/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        dto!.Id.Should().Be(product.Id);
        dto.Name.Should().Be("Detail Widget");
        dto.Price.Should().Be(49.99m);
        dto.IsActive.Should().BeTrue();
    }

    // ── PUT /api/v1/products/{id}/deactivate ──────────────────────────────────

    [Fact]
    public async Task Deactivate_ActiveProduct_Returns204()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("DEA")).Build();
        Seed(product);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{product.Id}/deactivate", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deactivate_AlreadyInactiveProduct_Returns409()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("DAI")).BuildInactive();
        Seed(product);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{product.Id}/deactivate", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── PUT /api/v1/products/{id}/price ──────────────────────────────────────

    [Fact]
    public async Task UpdatePrice_ValidPrice_Returns204()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("PRC")).Build();
        Seed(product);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{product.Id}/price",
                new { price = 99.99m, currency = "USD" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdatePrice_ZeroPrice_Returns400()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("ZPR")).Build();
        Seed(product);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{product.Id}/price",
                new { price = 0m, currency = "USD" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET with filters ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsOnlyMatchingProducts()
    {
        var uniqueName = $"UniqueSearch-{Guid.NewGuid().ToString("N")[..8]}";
        var product = new ProductBuilder()
            .WithName(uniqueName)
            .WithSku(UniqueSku("US"))
            .Build();
        Seed(product);

        var response = await CreateAdminClient()
            .GetAsync($"{Base}?search={uniqueName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ProductListDto>>();
        list!.Should().ContainSingle(p => p.Id == product.Id);
    }

    // ── Local DTOs ────────────────────────────────────────────────────────────

    private record ProductDetailDto(Guid Id, string Name, string Sku,
        string? Description, decimal Price, string Currency,
        string Category, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);

    private record ProductListDto(Guid Id, string Name, string Sku,
        decimal Price, string Currency, string Category, bool IsActive);
}
