using FluentAssertions;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.Unit.Domain;

public class CustomerTests
{
    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public void Register_WithValidData_SetsPropertiesCorrectly()
    {
        var customer = Customer.Register("John", "Doe", "John.Doe@Example.COM", "+1 555-1234");

        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be("john.doe@example.com"); // lowercased
        customer.Phone.Should().Be("+1 555-1234");
        customer.IsActive.Should().BeTrue();
        customer.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Register_WithoutPhone_PhoneIsNull()
    {
        var customer = Customer.Register("Jane", "Doe", "jane@example.com");

        customer.Phone.Should().BeNull();
    }

    [Fact]
    public void Register_FullName_CombinesFirstAndLastName()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        customer.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("A")] // less than 2 chars
    public void Register_WithInvalidFirstName_ThrowsDomainException(string firstName)
    {
        var act = () => Customer.Register(firstName, "Doe", "john@example.com");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("B")]
    public void Register_WithInvalidLastName_ThrowsDomainException(string lastName)
    {
        var act = () => Customer.Register("John", lastName, "john@example.com");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("")]
    public void Register_WithInvalidEmail_ThrowsDomainException(string email)
    {
        var act = () => Customer.Register("John", "Doe", email);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("abc")]    // too short / invalid format
    [InlineData("ABCDEF")] // no digits
    public void Register_WithInvalidPhone_ThrowsDomainException(string phone)
    {
        var act = () => Customer.Register("John", "Doe", "john@example.com", phone);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Register_RaisesCustomerRegisteredEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerRegisteredEvent);
    }

    // ── UpdateName ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateName_WithValidNames_UpdatesFirstAndLastName()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        customer.UpdateName("Jane", "Smith");

        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
    }

    [Fact]
    public void UpdateName_WithShortName_ThrowsDomainException()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        var act = () => customer.UpdateName("J", "Doe");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateName_RaisesCustomerUpdatedEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.ClearDomainEvents();

        customer.UpdateName("Jane", "Smith");

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerUpdatedEvent);
    }

    // ── ChangeEmail ───────────────────────────────────────────────────────────

    [Fact]
    public void ChangeEmail_ToValidEmail_UpdatesEmailLowercased()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        customer.ChangeEmail("NEWEMAIL@EXAMPLE.COM");

        customer.Email.Should().Be("newemail@example.com");
    }

    [Fact]
    public void ChangeEmail_ToSameEmail_DoesNotRaiseEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.ClearDomainEvents();

        customer.ChangeEmail("john@example.com"); // same, no-op

        customer.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeEmail_ToDifferentEmail_RaisesCustomerEmailChangedEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.ClearDomainEvents();

        customer.ChangeEmail("new@example.com");

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerEmailChangedEvent);
    }

    [Fact]
    public void ChangeEmail_ToInvalidEmail_ThrowsDomainException()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        var act = () => customer.ChangeEmail("not-an-email");

        act.Should().Throw<DomainException>();
    }

    // ── UpdateShippingAddress ─────────────────────────────────────────────────

    [Fact]
    public void UpdateShippingAddress_WithValidAddress_SetsAllAddressFields()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        var address = new Address("123 Main St", "Springfield", "IL", "62701", "US");

        customer.UpdateShippingAddress(address);

        customer.ShippingStreet.Should().Be("123 Main St");
        customer.ShippingCity.Should().Be("Springfield");
        customer.ShippingState.Should().Be("IL");
        customer.ShippingZipCode.Should().Be("62701");
        customer.ShippingCountry.Should().Be("US");
        customer.ShippingAddress.Should().NotBeNull();
    }

    [Fact]
    public void ClearShippingAddress_AfterSettingAddress_AllShippingFieldsAreNull()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.UpdateShippingAddress(new Address("123 Main St", "Springfield", "IL", "62701", "US"));

        customer.ClearShippingAddress();

        customer.ShippingStreet.Should().BeNull();
        customer.ShippingAddress.Should().BeNull();
    }

    // ── Deactivate / Reactivate ───────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveCustomer_IsActiveBecomesFalse()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactiveCustomer_ThrowsDomainException()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.Deactivate();

        var act = () => customer.Deactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_RaisesCustomerDeactivatedEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.ClearDomainEvents();

        customer.Deactivate();

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerDeactivatedEvent);
    }

    [Fact]
    public void Reactivate_InactiveCustomer_IsActiveBecomeTrue()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.Deactivate();

        customer.Reactivate();

        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_AlreadyActiveCustomer_ThrowsDomainException()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        var act = () => customer.Reactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reactivate_RaisesCustomerReactivatedEvent()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.Deactivate();
        customer.ClearDomainEvents();

        customer.Reactivate();

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerReactivatedEvent);
    }

    // ── EnsureCanPlaceOrder ───────────────────────────────────────────────────

    [Fact]
    public void EnsureCanPlaceOrder_ActiveCustomer_DoesNotThrow()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");

        var act = () => customer.EnsureCanPlaceOrder();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanPlaceOrder_InactiveCustomer_ThrowsDomainException()
    {
        var customer = Customer.Register("John", "Doe", "john@example.com");
        customer.Deactivate();

        var act = () => customer.EnsureCanPlaceOrder();

        act.Should().Throw<DomainException>();
    }
}
