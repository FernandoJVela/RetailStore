using FluentAssertions;
using RetailStore.Api.Features.Orders.Application.Queries;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers;

namespace RetailStore.Tests.Unit.Handlers.Queries;

public class OrderQueryHandlerTests
{
    // ── GetOrdersHandler ──────────────────────────────────────────────────────

    public class GetOrdersHandlerTests
    {
        [Fact]
        public async Task Handle_NoOrders_ReturnsEmptyList()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetOrdersHandler(db);

            var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_MultipleOrders_ReturnsAll()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(MakeDraftOrder());
            db.Add(MakeConfirmedOrder());
            db.SaveChanges();

            var handler = new GetOrdersHandler(db);
            var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_FilterByStatus_ReturnsOnlyMatchingOrders()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(MakeDraftOrder());
            db.Add(MakeConfirmedOrder());
            db.SaveChanges();

            var handler = new GetOrdersHandler(db);
            var result = await handler.Handle(new GetOrdersQuery(Status: "Confirmed"), CancellationToken.None);

            result.Should().ContainSingle();
            result[0].Status.Should().Be("Confirmed");
        }

        [Fact]
        public async Task Handle_FilterByUnknownStatus_ReturnsAll()
        {
            using var db = InMemoryDbContextFactory.Create();
            db.Add(MakeDraftOrder());
            db.SaveChanges();

            var handler = new GetOrdersHandler(db);
            // "NotAStatus" fails TryParse, so no filter is applied
            var result = await handler.Handle(new GetOrdersQuery(Status: "NotAStatus"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_OrderWithItems_ReturnsCorrectTotalAmountAndItemCount()
        {
            using var db = InMemoryDbContextFactory.Create();
            var customerId = Guid.NewGuid();
            var order = Api.Features.Orders.Domain.Order.Create(customerId);
            order.AddItem(Guid.NewGuid(), 2, new Money(10m, "USD")); // 20
            order.AddItem(Guid.NewGuid(), 3, new Money(5m, "USD"));  // 15
            db.Add(order);
            db.SaveChanges();

            var handler = new GetOrdersHandler(db);
            var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

            var dto = result.Single();
            dto.TotalAmount.Should().Be(35m);
            dto.ItemCount.Should().Be(2);
            dto.CustomerId.Should().Be(customerId);
            dto.Status.Should().Be("Draft");
        }
    }

    // ── GetOrderByIdHandler ───────────────────────────────────────────────────

    public class GetOrderByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingOrder_ReturnsOrderDetailWithItems()
        {
            using var db = InMemoryDbContextFactory.Create();
            var productId = Guid.NewGuid();
            var order = Api.Features.Orders.Domain.Order.Create(Guid.NewGuid());
            order.AddItem(productId, 3, new Money(15m, "USD"));
            db.Add(order);
            db.SaveChanges();

            var handler = new GetOrderByIdHandler(db);
            var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

            result.Id.Should().Be(order.Id);
            result.Items.Should().ContainSingle();
            result.Items[0].ProductId.Should().Be(productId);
            result.Items[0].Quantity.Should().Be(3);
            result.Items[0].UnitPrice.Should().Be(15m);
            result.Items[0].Subtotal.Should().Be(45m);
            result.TotalAmount.Should().Be(45m);
        }

        [Fact]
        public async Task Handle_NonExistentOrder_ThrowsDomainException()
        {
            using var db = InMemoryDbContextFactory.Create();
            var handler = new GetOrderByIdHandler(db);

            var act = () => handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_ConfirmedOrder_StatusIsConfirmedInDto()
        {
            using var db = InMemoryDbContextFactory.Create();
            var order = MakeConfirmedOrder();
            db.Add(order);
            db.SaveChanges();

            var handler = new GetOrderByIdHandler(db);
            var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

            result.Status.Should().Be(OrderStatus.Confirmed.ToString());
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Api.Features.Orders.Domain.Order MakeDraftOrder()
    {
        var order = Api.Features.Orders.Domain.Order.Create(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1, new Money(10m, "USD"));
        return order;
    }

    private static Api.Features.Orders.Domain.Order MakeConfirmedOrder()
    {
        var order = MakeDraftOrder();
        order.Confirm();
        return order;
    }
}
