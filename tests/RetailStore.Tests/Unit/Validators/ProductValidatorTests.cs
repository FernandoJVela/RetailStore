using FluentAssertions;
using RetailStore.Api.Features.Products.Application.Commands;

namespace RetailStore.Tests.Unit.Validators;

public class ProductValidatorTests
{
    // ── CreateProductValidator ────────────────────────────────────────────────

    public class CreateProductValidatorTests
    {
        private readonly CreateProductValidator _validator = new();

        private static CreateProductCommand ValidCommand() =>
            new("Widget Pro", "WGT-001", 29.99m, "USD", "Electronics", "A great widget");

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithoutDescription_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Description = null });

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankName_FailsValidation(string name)
        {
            var result = _validator.Validate(ValidCommand() with { Name = name });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Validate_NameExceedsMaxLength_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Name = new string('A', 201) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankSku_FailsValidation(string sku)
        {
            var result = _validator.Validate(ValidCommand() with { Sku = sku });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Sku");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99.99)]
        public void Validate_NonPositivePrice_FailsValidation(decimal price)
        {
            var result = _validator.Validate(ValidCommand() with { Price = price });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Price");
        }

        [Theory]
        [InlineData("")]
        [InlineData("US")]    // too short — must be exactly 3
        [InlineData("USDA")]  // too long
        public void Validate_InvalidCurrency_FailsValidation(string currency)
        {
            var result = _validator.Validate(ValidCommand() with { Currency = currency });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Currency");
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        public void Validate_ExactlyThreeLetterCurrency_Passes(string currency)
        {
            var result = _validator.Validate(ValidCommand() with { Currency = currency });

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankCategory_FailsValidation(string category)
        {
            var result = _validator.Validate(ValidCommand() with { Category = category });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Category");
        }

        [Fact]
        public void Validate_DescriptionExceedsMaxLength_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Description = new string('x', 2001) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Description");
        }

        [Fact]
        public void Validate_DescriptionAtMaxLength_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Description = new string('x', 2000) });

            result.IsValid.Should().BeTrue();
        }
    }

    // ── UpdateProductDetailsValidator ─────────────────────────────────────────

    public class UpdateProductDetailsValidatorTests
    {
        private readonly UpdateProductDetailsValidator _validator = new();

        private static UpdateProductDetailsCommand ValidCommand() =>
            new(Guid.NewGuid(), "Widget Pro", "Electronics", "Updated description");

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { ProductId = Guid.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }

        [Fact]
        public void Validate_BlankName_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Name = "" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Validate_BlankCategory_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Category = "" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Category");
        }

        [Fact]
        public void Validate_NullDescription_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Description = null });

            result.IsValid.Should().BeTrue();
        }
    }

    // ── UpdateProductPriceValidator ───────────────────────────────────────────

    public class UpdateProductPriceValidatorTests
    {
        private readonly UpdateProductPriceValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new UpdateProductPriceCommand(Guid.NewGuid(), 49.99m, "USD"));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new UpdateProductPriceCommand(Guid.Empty, 49.99m, "USD"));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        public void Validate_NonPositivePrice_FailsValidation(decimal price)
        {
            var result = _validator.Validate(new UpdateProductPriceCommand(Guid.NewGuid(), price, "USD"));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Price");
        }

        [Theory]
        [InlineData("US")]
        [InlineData("EURO")]
        [InlineData("")]
        public void Validate_InvalidCurrencyLength_FailsValidation(string currency)
        {
            var result = _validator.Validate(new UpdateProductPriceCommand(Guid.NewGuid(), 10m, currency));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Currency");
        }
    }

    // ── DeactivateProductValidator ────────────────────────────────────────────

    public class DeactivateProductValidatorTests
    {
        private readonly DeactivateProductValidator _validator = new();

        [Fact]
        public void Validate_ValidProductId_ReturnsNoErrors()
        {
            var result = _validator.Validate(new DeactivateProductCommand(Guid.NewGuid()));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyProductId_FailsValidation()
        {
            var result = _validator.Validate(new DeactivateProductCommand(Guid.Empty));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
        }
    }
}
