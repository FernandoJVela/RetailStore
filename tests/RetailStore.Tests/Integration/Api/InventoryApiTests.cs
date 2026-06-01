using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Api;

/// <summary>
/// HTTP-level tests for InventoryController (api/v1/inventory).
/// Products must exist before creating inventory records because the handler validates them.
/// </summary>
public class InventoryApiTests : IntegrationTestBase, IClassFixture<RetailStoreWebAppFactory>
{
    private const string Base = "/api/v1/inventory";

    private static string UniqueSku(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    public InventoryApiTests(RetailStoreWebAppFactory factory) : base(factory) { }

    // ── POST /api/v1/inventory ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ExistingProduct_Returns201WithLocationHeader()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("INV")).Build();
        Seed(product);

        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            productId = product.Id,
            initialQuantity = 50,
            reorderThreshold = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ProductNotFound_Returns404()
    {
        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            productId = Guid.NewGuid(),
            initialQuantity = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_DuplicateProduct_Returns409()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("DUP")).Build();
        Seed(product);
        var body = new { productId = product.Id, initialQuantity = 10 };

        var client = CreateAdminClient();
        (await client.PostAsJsonAsync(Base, body)).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync(Base, body);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_NegativeInitialQuantity_Returns400()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("NEG")).Build();
        Seed(product);

        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            productId = product.Id,
            initialQuantity = -1
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/v1/inventory ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns200WithList()
    {
        var response = await CreateAdminClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithStockStatusFilter_ReturnsOnlyMatchingItems()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("LST")).Build();
        var item = new InventoryItemBuilder()
            .ForProduct(product.Id).WithQuantity(3).WithThreshold(10).Build();
        Seed(product, item);

        var response = await CreateAdminClient().GetAsync($"{Base}?stockStatus=LowStock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>();
        items!.Should().Contain(i => i.ProductId == product.Id);
    }

    // ── GET /api/v1/inventory/{productId} ────────────────────────────────────

    [Fact]
    public async Task GetByProduct_ExistingInventory_Returns200WithDetail()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("GBP")).Build();
        var item = new InventoryItemBuilder()
            .ForProduct(product.Id).WithQuantity(25).WithThreshold(5).Build();
        Seed(product, item);

        var response = await CreateAdminClient().GetAsync($"{Base}/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<InventoryDetailDto>();
        detail!.ProductId.Should().Be(product.Id);
        detail.QuantityOnHand.Should().Be(25);
        detail.StockStatus.Should().Be("InStock");
    }

    [Fact]
    public async Task GetByProduct_NoInventoryForProduct_Returns404()
    {
        var response = await CreateAdminClient().GetAsync($"{Base}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/v1/inventory/low-stock ──────────────────────────────────────

    [Fact]
    public async Task GetLowStock_Returns200()
    {
        var response = await CreateAdminClient().GetAsync($"{Base}/low-stock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── PUT /api/v1/inventory/{productId}/add-stock ───────────────────────────

    [Fact]
    public async Task AddStock_ExistingInventory_Returns204()
    {
        var (productId, _) = await CreateInventoryAsync(initialQuantity: 20);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{productId}/add-stock",
                new { productId, quantity = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddStock_NoInventoryForProduct_Returns404()
    {
        var missingId = Guid.NewGuid();
        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{missingId}/add-stock",
                new { productId = missingId, quantity = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/v1/inventory/{productId}/adjust ──────────────────────────────

    [Fact]
    public async Task Adjust_ValidNewQuantity_Returns204()
    {
        var (productId, _) = await CreateInventoryAsync(initialQuantity: 50);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{productId}/adjust",
                new { productId, newQuantity = 30, reason = "physical count" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Adjust_NegativeQuantity_Returns400()
    {
        var (productId, _) = await CreateInventoryAsync(initialQuantity: 20);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{productId}/adjust",
                new { productId, newQuantity = -1, reason = "error" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /api/v1/inventory/{productId}/remove-stock ────────────────────────

    [Fact]
    public async Task RemoveStock_SufficientQuantity_Returns204()
    {
        var (productId, _) = await CreateInventoryAsync(initialQuantity: 30);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{productId}/remove-stock",
                new { productId, quantity = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveStock_MoreThanOnHand_Returns422()
    {
        var (productId, _) = await CreateInventoryAsync(initialQuantity: 5);

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{productId}/remove-stock",
                new { productId, quantity = 100 });

        // InsufficientStock → BusinessRule → 422
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private async Task<(Guid productId, Guid inventoryId)> CreateInventoryAsync(
        int initialQuantity = 50)
    {
        var product = new ProductBuilder().WithSku(UniqueSku("INV")).Build();
        Seed(product);

        var resp = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            productId = product.Id,
            initialQuantity,
            reorderThreshold = 10
        });
        resp.EnsureSuccessStatusCode();
        var inventoryId = await resp.Content.ReadFromJsonAsync<Guid>();
        return (product.Id, inventoryId);
    }

    // ── Local DTOs ────────────────────────────────────────────────────────────

    private record InventoryItemDto(Guid Id, Guid ProductId, string ProductName,
        string Sku, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity,
        int ReorderThreshold, string StockStatus);

    private record InventoryDetailDto(Guid Id, Guid ProductId, string ProductName,
        string Sku, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity,
        int ReorderThreshold, string StockStatus, DateTime CreatedAt, DateTime? UpdatedAt);
}
