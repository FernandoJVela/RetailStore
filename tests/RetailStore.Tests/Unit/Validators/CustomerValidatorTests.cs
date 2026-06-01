using FluentAssertions;
using RetailStore.Api.Features.Customers.Application.Commands;

namespace RetailStore.Tests.Unit.Validators;

public class CustomerValidatorTests
{
    // ── RegisterCustomerValidator ─────────────────────────────────────────────

    public class RegisterCustomerValidatorTests
    {
        private readonly RegisterCustomerValidator _validator = new();

        private static RegisterCustomerCommand ValidCommand() =>
            new("John", "Doe", "john@example.com");

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithOptionalPhone_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Phone = "+1 555-1234" });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithFullShippingAddress_Passes()
        {
            var address = new ShippingAddressDto("123 Main St", "Springfield", "IL", "62701", "US");
            var result = _validator.Validate(ValidCommand() with { ShippingAddress = address });

            result.IsValid.Should().BeTrue();
        }

        // ── Name validation ──

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankFirstName_FailsValidation(string firstName)
        {
            var result = _validator.Validate(ValidCommand() with { FirstName = firstName });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        [Fact]
        public void Validate_FirstNameTooShort_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { FirstName = "A" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        [Fact]
        public void Validate_FirstNameExceedsMaxLength_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { FirstName = new string('A', 101) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        [Fact]
        public void Validate_LastNameTooShort_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { LastName = "B" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "LastName");
        }

        // ── Email validation ──

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankEmail_FailsValidation(string email)
        {
            var result = _validator.Validate(ValidCommand() with { Email = email });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Theory]
        [InlineData("notanemail")]
        [InlineData("missing@")]
        [InlineData("@nodomain.com")]
        public void Validate_InvalidEmailFormat_FailsValidation(string email)
        {
            var result = _validator.Validate(ValidCommand() with { Email = email });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmailExceedsMaxLength_FailsValidation()
        {
            var longEmail = new string('a', 252) + "@b.co"; // 257 chars — exceeds 256 max
            var result = _validator.Validate(ValidCommand() with { Email = longEmail });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        // ── Phone validation ──

        [Fact]
        public void Validate_NullPhone_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Phone = null });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_PhoneExceedsMaxLength_FailsValidation()
        {
            var longPhone = new string('1', 21);
            var result = _validator.Validate(ValidCommand() with { Phone = longPhone });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Phone");
        }

        // ── Conditional shipping address validation ──

        [Fact]
        public void Validate_ShippingAddressWithEmptyStreet_FailsValidation()
        {
            var address = new ShippingAddressDto("", "Springfield", "IL", "62701", "US");
            var result = _validator.Validate(ValidCommand() with { ShippingAddress = address });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Street"));
        }

        [Fact]
        public void Validate_ShippingAddressWithEmptyCity_FailsValidation()
        {
            var address = new ShippingAddressDto("123 Main St", "", "IL", "62701", "US");
            var result = _validator.Validate(ValidCommand() with { ShippingAddress = address });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("City"));
        }

        [Fact]
        public void Validate_ShippingAddressWithEmptyCountry_FailsValidation()
        {
            var address = new ShippingAddressDto("123 Main St", "Springfield", "IL", "62701", "");
            var result = _validator.Validate(ValidCommand() with { ShippingAddress = address });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Country"));
        }

        [Fact]
        public void Validate_NullShippingAddress_SkipsAddressRules()
        {
            // When no address is provided, address field rules should NOT fire
            var result = _validator.Validate(ValidCommand() with { ShippingAddress = null });

            result.IsValid.Should().BeTrue();
        }
    }

    // ── UpdateCustomerValidator ───────────────────────────────────────────────

    public class UpdateCustomerValidatorTests
    {
        private readonly UpdateCustomerValidator _validator = new();

        private static UpdateCustomerCommand ValidCommand() =>
            new(Guid.NewGuid(), "Jane", "Smith");

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyCustomerId_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { CustomerId = Guid.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        }

        [Fact]
        public void Validate_FirstNameTooShort_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { FirstName = "J" });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        [Fact]
        public void Validate_NullPhone_Passes()
        {
            var result = _validator.Validate(ValidCommand() with { Phone = null });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_PhoneTooLong_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Phone = new string('1', 21) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Phone");
        }
    }

    // ── ChangeCustomerEmailValidator ──────────────────────────────────────────

    public class ChangeCustomerEmailValidatorTests
    {
        private readonly ChangeCustomerEmailValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(new ChangeCustomerEmailCommand(Guid.NewGuid(), "new@example.com"));

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyCustomerId_FailsValidation()
        {
            var result = _validator.Validate(new ChangeCustomerEmailCommand(Guid.Empty, "new@example.com"));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        }

        [Theory]
        [InlineData("")]
        [InlineData("notanemail")]
        [InlineData("missing.domain")]
        public void Validate_InvalidEmail_FailsValidation(string email)
        {
            var result = _validator.Validate(new ChangeCustomerEmailCommand(Guid.NewGuid(), email));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "NewEmail");
        }
    }

    // ── UpdateShippingAddressValidator ────────────────────────────────────────

    public class UpdateShippingAddressValidatorTests
    {
        private readonly UpdateShippingAddressValidator _validator = new();

        private static UpdateShippingAddressCommand ValidCommand() =>
            new(Guid.NewGuid(), "123 Main St", "Springfield", "IL", "62701", "US");

        [Fact]
        public void Validate_ValidCommand_ReturnsNoErrors()
        {
            var result = _validator.Validate(ValidCommand());

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyCustomerId_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { CustomerId = Guid.Empty });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
        }

        [Theory]
        [InlineData("Street")]
        [InlineData("City")]
        [InlineData("Country")]
        public void Validate_RequiredAddressFieldEmpty_FailsValidation(string fieldName)
        {
            var cmd = fieldName switch
            {
                "Street"  => ValidCommand() with { Street = "" },
                "City"    => ValidCommand() with { City = "" },
                "Country" => ValidCommand() with { Country = "" },
                _         => ValidCommand()
            };

            var result = _validator.Validate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == fieldName);
        }

        [Fact]
        public void Validate_EmptyStateAndZipCode_Passes()
        {
            // State and ZipCode are optional
            var result = _validator.Validate(ValidCommand() with { State = "", ZipCode = "" });

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_StreetExceedsMaxLength_FailsValidation()
        {
            var result = _validator.Validate(ValidCommand() with { Street = new string('x', 301) });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Street");
        }
    }
}
