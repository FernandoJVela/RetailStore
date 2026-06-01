using FluentAssertions;
using Moq;
using RetailStore.Api.Features.Customers.Application;
using RetailStore.Api.Features.Customers.Application.Commands;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Tests.Unit.Handlers.Commands;

public class CustomerCommandHandlerTests
{
    // ─── RegisterCustomerHandler ─────────────────────────────────────────────

    public class RegisterCustomerHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customers = new();
        private readonly RegisterCustomerHandler _handler;

        public RegisterCustomerHandlerTests()
        {
            _customers.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            _handler = new RegisterCustomerHandler(_customers.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewCustomerId()
        {
            var cmd = new RegisterCustomerCommand("John", "Doe", "john@example.com");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsCustomerViaRepository()
        {
            var cmd = new RegisterCustomerCommand("John", "Doe", "john@example.com");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            await _handler.Handle(cmd, CancellationToken.None);

            _customers.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyTaken_ThrowsDomainException()
        {
            var cmd = new RegisterCustomerCommand("John", "Doe", "taken@example.com");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

            var act = () => _handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyTaken_DoesNotPersistCustomer()
        {
            var cmd = new RegisterCustomerCommand("John", "Doe", "taken@example.com");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

            try { await _handler.Handle(cmd, CancellationToken.None); } catch { }

            _customers.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithShippingAddress_CustomerHasShippingAddressSet()
        {
            Customer? capturedCustomer = null;
            _customers.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .Callback<Customer, CancellationToken>((c, _) => capturedCustomer = c)
                      .Returns(Task.CompletedTask);

            var address = new ShippingAddressDto("123 Main St", "Springfield", "IL", "62701", "US");
            var cmd = new RegisterCustomerCommand("John", "Doe", "john@example.com", Phone: null, ShippingAddress: address);

            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            await _handler.Handle(cmd, CancellationToken.None);

            capturedCustomer!.ShippingAddress.Should().NotBeNull();
            capturedCustomer.ShippingStreet.Should().Be("123 Main St");
            capturedCustomer.ShippingCity.Should().Be("Springfield");
            capturedCustomer.ShippingCountry.Should().Be("US");
        }

        [Fact]
        public async Task Handle_WithoutShippingAddress_CustomerHasNoShippingAddress()
        {
            Customer? capturedCustomer = null;
            _customers.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .Callback<Customer, CancellationToken>((c, _) => capturedCustomer = c)
                      .Returns(Task.CompletedTask);

            var cmd = new RegisterCustomerCommand("John", "Doe", "john@example.com");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            await _handler.Handle(cmd, CancellationToken.None);

            capturedCustomer!.ShippingAddress.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_EmailIsStoredLowercased()
        {
            Customer? capturedCustomer = null;
            _customers.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .Callback<Customer, CancellationToken>((c, _) => capturedCustomer = c)
                      .Returns(Task.CompletedTask);

            var cmd = new RegisterCustomerCommand("John", "Doe", "John.Doe@Example.COM");
            _customers.Setup(r => r.ExistsWithEmailAsync(cmd.Email, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            await _handler.Handle(cmd, CancellationToken.None);

            capturedCustomer!.Email.Should().Be("john.doe@example.com");
        }
    }
}
