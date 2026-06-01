using FluentAssertions;
using RetailStore.Api.Features.Inventory.Application.Commands;

namespace RetailStore.Tests.Unit.Validators;

public class InventoryValidatorTests
{
    // ── CreateInventoryItemValidator ──────────────────────────────────────────

    public class CreateInventoryItemValidatorTests
    {
        private readonly CreateInventoryItemValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.NewGuid(), 50, 10));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ZeroInitialQuantity_Passes()
        {
            // Zero is allowed (no stock yet but tracking the product)
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.NewGuid(), 0, 10));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.Empty, 50, 10));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }

        [Fact]
        public void Validate_NegativeInitialQuantity_FailsValidation()
        {
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.NewGuid(), -1, 10));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "InitialQuantity");
        }

        [Fact]
        public void Validate_NegativeReorderThreshold_FailsValidation()
        {
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.NewGuid(), 50, -1));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ReorderThreshold");
        }

        [Fact]
        public void Validate_ZeroReorderThreshold_Passes()
        {
            var result = _validator.Validate(new CreateInventoryItemCommand(Guid.NewGuid(), 50, 0));

            result.IsValid.Should().BeTrue();
        }
    }

    // ── AddStockValidator ─────────────────────────────────────────────────────

    public class AddStockValidatorTests
    {
        private readonly AddStockValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new AddStockCommand(Guid.NewGuid(), 10));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new AddStockCommand(Guid.Empty, 10));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_NonPositiveQuantity_FailsValidation(int quantity)
        {
            var result = _validator.Validate(new AddStockCommand(Guid.NewGuid(), quantity));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
        }
    }

    // ── RemoveStockValidator ──────────────────────────────────────────────────

    public class RemoveStockValidatorTests
    {
        private readonly RemoveStockValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new RemoveStockCommand(Guid.NewGuid(), 5));

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_NonPositiveQuantity_FailsValidation(int quantity)
        {
            var result = _validator.Validate(new RemoveStockCommand(Guid.NewGuid(), quantity));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
        }
    }

    // ── AdjustStockValidator ──────────────────────────────────────────────────

    public class AdjustStockValidatorTests
    {
        private readonly AdjustStockValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new AdjustStockCommand(Guid.NewGuid(), 30, "physical count"));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ZeroNewQuantity_Passes()
        {
            // Adjusting to 0 is valid (writing off all stock)
            var result = _validator.Validate(new AdjustStockCommand(Guid.NewGuid(), 0, "write-off"));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_NegativeNewQuantity_FailsValidation()
        {
            var result = _validator.Validate(new AdjustStockCommand(Guid.NewGuid(), -1, "error"));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NewQuantity");
        }

        [Fact]
        public void Validate_EmptyReason_FailsValidation()
        {
            var result = _validator.Validate(new AdjustStockCommand(Guid.NewGuid(), 30, ""));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Validate_ReasonExceedsMaxLength_FailsValidation()
        {
            var result = _validator.Validate(new AdjustStockCommand(Guid.NewGuid(), 30, new string('x', 501)));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Reason");
        }
    }

    // ── UpdateReorderThresholdValidator ───────────────────────────────────────

    public class UpdateReorderThresholdValidatorTests
    {
        private readonly UpdateReorderThresholdValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new UpdateReorderThresholdCommand(Guid.NewGuid(), 15));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ZeroThreshold_Passes()
        {
            // Threshold of 0 disables low-stock alerts — valid
            var result = _validator.Validate(new UpdateReorderThresholdCommand(Guid.NewGuid(), 0));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_NegativeThreshold_FailsValidation()
        {
            var result = _validator.Validate(new UpdateReorderThresholdCommand(Guid.NewGuid(), -1));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NewThreshold");
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new UpdateReorderThresholdCommand(Guid.Empty, 10));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }
    }
}
