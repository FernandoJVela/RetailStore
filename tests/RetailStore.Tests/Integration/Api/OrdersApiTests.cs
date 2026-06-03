using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Integration.Api;

/// <summary>
/// HTTP-level tests for OrdersController (api/v1/orders).
/// Exercises the full order lifecycle: create → confirm → ship/deliver → complete / cancel.
/// Products are seeded directly because the order handler fetches them by ID.
/// </summary>
public class OrdersApiTests : IntegrationTestBase, IClassFixture<RetailStoreWebAppFactory>
{
    private const string Base = "/api/v1/orders";

    private static string UniqueSku(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    public OrdersApiTests(RetailStoreWebAppFactory factory) : base(factory) { }

    // ── POST /api/v1/orders ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidOrderWithExistingProduct_Returns201()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("ORD")).Build();
        Seed(product);

        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = new[] { new { productId = product.Id, quantity = 2 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ProductNotFound_Returns404()
    {
        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = new[] { new { productId = Guid.NewGuid(), quantity = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_InactiveProduct_Returns409()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("INA")).BuildInactive();
        Seed(product);

        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = new[] { new { productId = product.Id, quantity = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_EmptyItemsList_Returns400()
    {
        var response = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AnonymousRequest_Returns401()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            items = new[] { new { productId = Guid.NewGuid(), quantity = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/orders ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        var response = await CreateAdminClient().GetAsync(Base);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderListDto>>();
        orders.Should().NotBeNull();
    }

    // ── GET /api/v1/orders/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsDetailDtoWithItems()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("GBO")).Build();
        Seed(product);

        var createResp = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = new[] { new { productId = product.Id, quantity = 3 } }
        });
        createResp.EnsureSuccessStatusCode();

        var orderId = ExtractIdFromLocation(createResp);
        var getResp = await CreateAdminClient().GetAsync($"{Base}/{orderId}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getResp.Content.ReadFromJsonAsync<OrderDetailDto>();
        detail!.Id.Should().Be(orderId);
        detail.Items.Should().ContainSingle();
        detail.Items[0].ProductId.Should().Be(product.Id);
        detail.Items[0].Quantity.Should().Be(3);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var response = await CreateAdminClient().GetAsync($"{Base}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/v1/orders/{id}/confirm ──────────────────────────────────────

    [Fact]
    public async Task Confirm_DraftOrderWithItems_Returns204()
    {
        var orderId = await CreateDraftOrderAsync();

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{orderId}/confirm", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Confirm_NonExistentOrder_Returns404()
    {
        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{Guid.NewGuid()}/confirm", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/v1/orders/{id}/complete ─────────────────────────────────────

    [Fact]
    public async Task Complete_ConfirmedOrder_Returns422()
    {
        // Completing a Confirmed order directly is not allowed — must be Delivered first.
        var orderId = await CreateDraftOrderAsync();
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"{Base}/{orderId}/confirm", new { });

        var response = await client.PutAsJsonAsync($"{Base}/{orderId}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Complete_DeliveredOrder_Returns204()
    {
        var orderId = await CreateDraftOrderAsync();
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"{Base}/{orderId}/confirm", new { });

        // Advance to Delivered via domain methods (bypasses outbox, which is disabled in tests)
        Factory.UseDbContext(db =>
        {
            var order = db.Set<Order>().First(o => o.Id == orderId);
            order.MarkShipped();
            order.MarkDelivered();
            db.SaveChanges();
        });

        var response = await client.PutAsJsonAsync($"{Base}/{orderId}/complete", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── PUT /api/v1/orders/{id}/cancel ────────────────────────────────────────

    [Fact]
    public async Task Cancel_DraftOrder_Returns204()
    {
        var orderId = await CreateDraftOrderAsync();

        var response = await CreateAdminClient()
            .PutAsJsonAsync($"{Base}/{orderId}/cancel",
                new { orderId, reason = "test cancellation" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cancel_CompletedOrder_Returns422()
    {
        var orderId = await CreateDraftOrderAsync();
        var client = CreateAdminClient();
        await client.PutAsJsonAsync($"{Base}/{orderId}/confirm", new { });

        // Advance to Completed via domain methods
        Factory.UseDbContext(db =>
        {
            var order = db.Set<Order>().First(o => o.Id == orderId);
            order.MarkShipped();
            order.MarkDelivered();
            order.Complete();
            db.SaveChanges();
        });

        var response = await client.PutAsJsonAsync(
            $"{Base}/{orderId}/cancel",
            new { orderId, reason = "too late" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> CreateDraftOrderAsync()
    {
        var product = new ProductBuilder().WithSku(UniqueSku("DFT")).Build();
        Seed(product);

        var resp = await CreateAdminClient().PostAsJsonAsync(Base, new
        {
            customerId = Guid.NewGuid(),
            orderDate = (DateTime?)null,
            items = new[] { new { productId = product.Id, quantity = 1 } }
        });
        resp.EnsureSuccessStatusCode();
        return ExtractIdFromLocation(resp);
    }

    // The Orders controller returns CreatedAtAction with body = null, so we read from Location.
    private static Guid ExtractIdFromLocation(HttpResponseMessage resp)
        => Guid.Parse(resp.Headers.Location!.ToString().Split('/').Last());

    // ── Local DTOs ────────────────────────────────────────────────────────────

    private record OrderListDto(Guid Id, Guid CustomerId, string Status,
        decimal TotalAmount, int ItemCount);

    private record OrderDetailDto(Guid Id, Guid CustomerId, string Status,
        decimal TotalAmount, List<OrderItemDto> Items);

    private record OrderItemDto(Guid Id, Guid ProductId, int Quantity,
        decimal UnitPrice, string Currency, decimal Subtotal);
}
