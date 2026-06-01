using FluentAssertions;
using Moq;
using RetailStore.Api.Features.Orders.Application.Commands;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
using RetailStore.SharedKernel.Domain.ValueObjects;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Unit.Handlers.Commands;

public class OrderCommandHandlerTests
{
    // ─── CreateOrderHandler ──────────────────────────────────────────────────

    public class CreateOrderHandlerTests
    {
        private readonly Mock<IRepository<Order>> _orders = new();
        private readonly Mock<IRepository<Product>> _products = new();
        private readonly CreateOrderHandler _handler;

        public CreateOrderHandlerTests()
        {
            _orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            _handler = new CreateOrderHandler(_orders.Object, _products.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewOrderId()
        {
            var product = new ProductBuilder().Build();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(product.Id, 2)]);

            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsOrderViaRepository()
        {
            var product = new ProductBuilder().Build();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(product.Id, 1)]);

            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);

            await _handler.Handle(cmd, CancellationToken.None);

            _orders.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithMultipleItems_FetchesEachProductFromRepository()
        {
            var product1 = new ProductBuilder().WithSku("P-001").Build();
            var product2 = new ProductBuilder().WithSku("P-002").Build();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(product1.Id, 1), new CreateOrderItemDto(product2.Id, 3)]);

            _products.Setup(r => r.GetByIdAsync(product1.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product1);
            _products.Setup(r => r.GetByIdAsync(product2.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product2);

            await _handler.Handle(cmd, CancellationToken.None);

            _products.Verify(r => r.GetByIdAsync(product1.Id, It.IsAny<CancellationToken>()), Times.Once);
            _products.Verify(r => r.GetByIdAsync(product2.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenProductNotFound_ThrowsDomainException()
        {
            var missingId = Guid.NewGuid();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(missingId, 1)]);

            _products.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var act = () => _handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_WhenProductIsInactive_ThrowsDomainException()
        {
            var product = new ProductBuilder().BuildInactive();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(product.Id, 1)]);

            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);

            var act = () => _handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_WhenProductIsInactive_DoesNotPersistOrder()
        {
            var product = new ProductBuilder().BuildInactive();
            var cmd = new CreateOrderCommand(
                Guid.NewGuid(),
                null,
                [new CreateOrderItemDto(product.Id, 1)]);

            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);

            try { await _handler.Handle(cmd, CancellationToken.None); } catch { }

            _orders.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    // ─── ConfirmOrderHandler ─────────────────────────────────────────────────

    public class ConfirmOrderHandlerTests
    {
        private readonly Mock<IRepository<Order>> _orders = new();
        private readonly ConfirmOrderHandler _handler;

        public ConfirmOrderHandlerTests() => _handler = new ConfirmOrderHandler(_orders.Object);

        [Fact]
        public async Task Handle_DraftOrderWithItems_OrderStatusBecomesConfirmed()
        {
            var order = new OrderBuilder().BuildWithItem();
            _orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public async Task Handle_WhenOrderNotFound_ThrowsDomainException()
        {
            _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Order?)null);

            var act = () => _handler.Handle(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── CompleteOrderHandler ────────────────────────────────────────────────

    public class CompleteOrderHandlerTests
    {
        private readonly Mock<IRepository<Order>> _orders = new();
        private readonly CompleteOrderHandler _handler;

        public CompleteOrderHandlerTests() => _handler = new CompleteOrderHandler(_orders.Object);

        [Fact]
        public async Task Handle_ConfirmedOrder_OrderStatusBecomesCompleted()
        {
            var order = new OrderBuilder().BuildConfirmed();
            _orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            await _handler.Handle(new CompleteOrderCommand(order.Id), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Completed);
            order.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WhenOrderNotFound_ThrowsDomainException()
        {
            _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Order?)null);

            var act = () => _handler.Handle(new CompleteOrderCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_DraftOrder_ThrowsDomainException()
        {
            var order = new OrderBuilder().BuildWithItem(); // Draft, not Confirmed
            _orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            var act = () => _handler.Handle(new CompleteOrderCommand(order.Id), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── CancelOrderHandler ──────────────────────────────────────────────────

    public class CancelOrderHandlerTests
    {
        private readonly Mock<IRepository<Order>> _orders = new();
        private readonly CancelOrderHandler _handler;

        public CancelOrderHandlerTests() => _handler = new CancelOrderHandler(_orders.Object);

        [Fact]
        public async Task Handle_DraftOrder_OrderStatusBecomesCancelled()
        {
            var order = new OrderBuilder().Build();
            _orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            await _handler.Handle(new CancelOrderCommand(order.Id, "customer request"), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Cancelled);
            order.CancelledAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WhenOrderNotFound_ThrowsDomainException()
        {
            _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Order?)null);

            var act = () => _handler.Handle(new CancelOrderCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_CompletedOrder_ThrowsDomainException()
        {
            var order = new OrderBuilder().BuildConfirmed();
            order.Complete();
            _orders.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(order);

            var act = () => _handler.Handle(new CancelOrderCommand(order.Id, "too late"), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
