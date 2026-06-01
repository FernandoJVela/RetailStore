using FluentAssertions;
using RetailStore.Api.Features.Orders.Application.Commands;

namespace RetailStore.Tests.Unit.Validators;

public class OrderValidatorTests
{
    // ── CreateOrderValidator ──────────────────────────────────────────────────

    public class CreateOrderValidatorTests
    {
        private readonly CreateOrderValidator _validator = new();

        private static CreateOrderCommand ValidCommand() => new(
            CustomerId: Guid.NewGuid(),
            OrderDate: null,
            Items: [new CreateOrderItemDto(Guid.NewGuid(), Quantity: 2)]
        );

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyCustomerId_FailsValidation()
        {
            var cmd = ValidCommand() with { CustomerId = Guid.Empty };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        }

        [Fact]
        public void Validate_EmptyItemsList_FailsValidation()
        {
            var cmd = ValidCommand() with { Items = [] };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Items");
        }

        [Fact]
        public void Validate_DuplicateProductIds_FailsValidation()
        {
            var sharedId = Guid.NewGuid();
            var cmd = ValidCommand() with
            {
                Items = [new CreateOrderItemDto(sharedId, 1), new CreateOrderItemDto(sharedId, 2)]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Items");
        }

        [Fact]
        public void Validate_ItemQuantityZero_FailsValidation()
        {
            var cmd = ValidCommand() with
            {
                Items = [new CreateOrderItemDto(Guid.NewGuid(), Quantity: 0)]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ItemQuantityExceedsMax_FailsValidation()
        {
            var cmd = ValidCommand() with
            {
                Items = [new CreateOrderItemDto(Guid.NewGuid(), Quantity: 101)]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ItemQuantityAtMaxBoundary_Passes()
        {
            var cmd = ValidCommand() with
            {
                Items = [new CreateOrderItemDto(Guid.NewGuid(), Quantity: 100)]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ItemWithEmptyProductId_FailsValidation()
        {
            var cmd = ValidCommand() with
            {
                Items = [new CreateOrderItemDto(Guid.Empty, Quantity: 1)]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_MultipleDistinctProducts_Passes()
        {
            var cmd = ValidCommand() with
            {
                Items =
                [
                    new CreateOrderItemDto(Guid.NewGuid(), 1),
                    new CreateOrderItemDto(Guid.NewGuid(), 5),
                    new CreateOrderItemDto(Guid.NewGuid(), 10)
                ]
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeTrue();
        }
    }

    // ── CancelOrderValidator ──────────────────────────────────────────────────

    public class CancelOrderValidatorTests
    {
        private readonly CancelOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), "customer request"));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyOrderId_FailsValidation()
        {
            var result = _validator.Validate(new CancelOrderCommand(Guid.Empty, "reason"));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
        }

        [Fact]
        public void Validate_EmptyReason_FailsValidation()
        {
            var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), ""));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Validate_ReasonExceedsMaxLength_FailsValidation()
        {
            var longReason = new string('x', 501);

            var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), longReason));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Validate_ReasonAtMaxLength_Passes()
        {
            var maxReason = new string('x', 500);

            var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), maxReason));

            result.IsValid.Should().BeTrue();
        }
    }

    // ── AddOrderItemValidator ─────────────────────────────────────────────────

    public class AddOrderItemValidatorTests
    {
        private readonly AddOrderItemValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), 3));

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_NonPositiveQuantity_FailsValidation(int quantity)
        {
            var result = _validator.Validate(new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), quantity));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
        }

        [Fact]
        public void Validate_EmptyOrderId_FailsValidation()
        {
            var result = _validator.Validate(new AddOrderItemCommand(Guid.Empty, Guid.NewGuid(), 1));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new AddOrderItemCommand(Guid.NewGuid(), Guid.Empty, 1));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }
    }
}
