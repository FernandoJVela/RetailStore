using FluentAssertions;
using Moq;
using RetailStore.Api.Features.Inventory.Application;
using RetailStore.Api.Features.Inventory.Application.Commands;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.Tests.TestHelpers.Builders;

namespace RetailStore.Tests.Unit.Handlers.Commands;

public class InventoryCommandHandlerTests
{
    // ─── CreateInventoryItemHandler ──────────────────────────────────────────

    public class CreateInventoryItemHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly Mock<IRepository<Product>> _products = new();
        private readonly CreateInventoryItemHandler _handler;

        public CreateInventoryItemHandlerTests()
        {
            _inventory.Setup(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            _handler = new CreateInventoryItemHandler(_inventory.Object, _products.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewInventoryItemId()
        {
            var product = new ProductBuilder().Build();
            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);
            _inventory.Setup(r => r.ExistsForProductAsync(product.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            var cmd = new CreateInventoryItemCommand(product.Id, 50, 10);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsInventoryItemViaRepository()
        {
            var product = new ProductBuilder().Build();
            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);
            _inventory.Setup(r => r.ExistsForProductAsync(product.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            var cmd = new CreateInventoryItemCommand(product.Id, 50, 10);

            await _handler.Handle(cmd, CancellationToken.None);

            _inventory.Verify(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenProductNotFound_ThrowsDomainException()
        {
            var missingId = Guid.NewGuid();
            _products.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var cmd = new CreateInventoryItemCommand(missingId, 50);

            var act = () => _handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_WhenInventoryAlreadyExistsForProduct_ThrowsDomainException()
        {
            var product = new ProductBuilder().Build();
            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);
            _inventory.Setup(r => r.ExistsForProductAsync(product.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true); // duplicate!

            var cmd = new CreateInventoryItemCommand(product.Id, 50);

            var act = () => _handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_WhenInventoryAlreadyExists_DoesNotPersistNewItem()
        {
            var product = new ProductBuilder().Build();
            _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(product);
            _inventory.Setup(r => r.ExistsForProductAsync(product.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

            try { await _handler.Handle(new CreateInventoryItemCommand(product.Id, 50), CancellationToken.None); } catch { }

            _inventory.Verify(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    // ─── AddStockHandler ─────────────────────────────────────────────────────

    public class AddStockHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly AddStockHandler _handler;

        public AddStockHandlerTests() => _handler = new AddStockHandler(_inventory.Object);

        private static InventoryItem MakeItem(int quantity = 20) =>
            InventoryItem.Create(Guid.NewGuid(), quantity);

        [Fact]
        public async Task Handle_WhenItemFound_IncreasesQuantityOnHand()
        {
            var item = MakeItem(20);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            await _handler.Handle(new AddStockCommand(item.ProductId, 10), CancellationToken.None);

            item.QuantityOnHand.Should().Be(30);
        }

        [Fact]
        public async Task Handle_WhenItemNotFound_ThrowsDomainException()
        {
            var productId = Guid.NewGuid();
            _inventory.Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((InventoryItem?)null);

            var act = () => _handler.Handle(new AddStockCommand(productId, 10), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── RemoveStockHandler ───────────────────────────────────────────────────

    public class RemoveStockHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly RemoveStockHandler _handler;

        public RemoveStockHandlerTests() => _handler = new RemoveStockHandler(_inventory.Object);

        [Fact]
        public async Task Handle_WhenItemFound_DecreasesQuantityOnHand()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 20);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            await _handler.Handle(new RemoveStockCommand(item.ProductId, 5), CancellationToken.None);

            item.QuantityOnHand.Should().Be(15);
        }

        [Fact]
        public async Task Handle_WhenItemNotFound_ThrowsDomainException()
        {
            var productId = Guid.NewGuid();
            _inventory.Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((InventoryItem?)null);

            var act = () => _handler.Handle(new RemoveStockCommand(productId, 5), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_RemovingMoreThanOnHand_ThrowsDomainException()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 5);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            var act = () => _handler.Handle(new RemoveStockCommand(item.ProductId, 100), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── ReserveStockHandler ─────────────────────────────────────────────────

    public class ReserveStockHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly ReserveStockHandler _handler;

        public ReserveStockHandlerTests() => _handler = new ReserveStockHandler(_inventory.Object);

        [Fact]
        public async Task Handle_WhenItemFound_IncreasesReservedQuantity()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 20);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            await _handler.Handle(new ReserveStockCommand(item.ProductId, 8), CancellationToken.None);

            item.ReservedQuantity.Should().Be(8);
            item.AvailableQuantity.Should().Be(12);
        }

        [Fact]
        public async Task Handle_ReservingMoreThanAvailable_ThrowsDomainException()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 5);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            var act = () => _handler.Handle(new ReserveStockCommand(item.ProductId, 10), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── AdjustStockHandler ───────────────────────────────────────────────────

    public class AdjustStockHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly AdjustStockHandler _handler;

        public AdjustStockHandlerTests() => _handler = new AdjustStockHandler(_inventory.Object);

        [Fact]
        public async Task Handle_WhenItemFound_SetsQuantityToNewValue()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 50);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            await _handler.Handle(new AdjustStockCommand(item.ProductId, 30, "physical count"), CancellationToken.None);

            item.QuantityOnHand.Should().Be(30);
        }

        [Fact]
        public async Task Handle_WhenItemNotFound_ThrowsDomainException()
        {
            var productId = Guid.NewGuid();
            _inventory.Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((InventoryItem?)null);

            var act = () => _handler.Handle(new AdjustStockCommand(productId, 30, "count"), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }

    // ─── FulfillReservationHandler ────────────────────────────────────────────

    public class FulfillReservationHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventory = new();
        private readonly FulfillReservationHandler _handler;

        public FulfillReservationHandlerTests() => _handler = new FulfillReservationHandler(_inventory.Object);

        [Fact]
        public async Task Handle_WhenItemFound_DecreasesReservedAndOnHand()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 20);
            item.Reserve(10);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            await _handler.Handle(new FulfillReservationCommand(item.ProductId, 10), CancellationToken.None);

            item.ReservedQuantity.Should().Be(0);
            item.QuantityOnHand.Should().Be(10);
        }

        [Fact]
        public async Task Handle_FulfillMoreThanReserved_ThrowsDomainException()
        {
            var item = InventoryItem.Create(Guid.NewGuid(), 20);
            item.Reserve(5);
            _inventory.Setup(r => r.GetByProductIdAsync(item.ProductId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(item);

            var act = () => _handler.Handle(new FulfillReservationCommand(item.ProductId, 100), CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
