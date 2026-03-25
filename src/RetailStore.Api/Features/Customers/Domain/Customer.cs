using System.Text.RegularExpressions;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Customers.Domain;
 
public sealed class Customer : AggregateRoot
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
 
    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);
 
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
 
    // Shipping Address stored as flat columns
    public string? ShippingStreet { get; private set; }
    public string? ShippingCity { get; private set; }
    public string? ShippingState { get; private set; }
    public string? ShippingZipCode { get; private set; }
    public string? ShippingCountry { get; private set; }
 
    public bool IsActive { get; private set; } = true;
 
    private Customer() { } // EF Core
 
    // ─── Computed Property ──────────────────────────────────
    public string FullName => $"{FirstName} {LastName}";
 
    public Address? ShippingAddress =>
        ShippingStreet is not null
            ? new Address(ShippingStreet, ShippingCity!, ShippingState!,
                ShippingZipCode!, ShippingCountry!)
            : null;
 
    // ─── Factory ────────────────────────────────────────────
    public static Customer Register(
        string firstName,
        string lastName,
        string email,
        string? phone = null)
    {
        ValidateName(firstName, "First name");
        ValidateName(lastName, "Last name");
        ValidateEmail(email);
 
        if (phone is not null)
            ValidatePhone(phone);
 
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            Phone = phone?.Trim()
        };
 
        customer.Raise(new CustomerRegisteredEvent(
            customer.Id, customer.FirstName, customer.LastName, customer.Email));
 
        return customer;
    }
 
    // ─── Profile Updates ────────────────────────────────────
    public void UpdateName(string firstName, string lastName)
    {
        ValidateName(firstName, "First name");
        ValidateName(lastName, "Last name");
 
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Touch();
 
        Raise(new CustomerUpdatedEvent(Id, FirstName, LastName));
    }
 
    public void ChangeEmail(string newEmail)
    {
        ValidateEmail(newEmail);
 
        var normalizedEmail = newEmail.ToLowerInvariant().Trim();
        if (normalizedEmail == Email) return;
 
        var oldEmail = Email;
        Email = normalizedEmail;
        Touch();
 
        Raise(new CustomerEmailChangedEvent(Id, oldEmail, Email));
    }
 
    public void UpdatePhone(string? phone)
    {
        if (phone is not null)
            ValidatePhone(phone);
 
        Phone = phone?.Trim();
        Touch();
    }
 
    // ─── Address Management ─────────────────────────────────
    public void UpdateShippingAddress(Address address)
    {
        ShippingStreet = address.Street;
        ShippingCity = address.City;
        ShippingState = address.State;
        ShippingZipCode = address.ZipCode;
        ShippingCountry = address.Country;
        Touch();
 
        Raise(new CustomerAddressUpdatedEvent(
            Id, address.Street, address.City,
            address.State, address.ZipCode, address.Country));
    }
 
    public void ClearShippingAddress()
    {
        ShippingStreet = null;
        ShippingCity = null;
        ShippingState = null;
        ShippingZipCode = null;
        ShippingCountry = null;
        Touch();
    }
 
    // ─── Lifecycle ──────────────────────────────────────────
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(CustomerErrors.AlreadyDeactivated());
 
        IsActive = false;
        Touch();
 
        Raise(new CustomerDeactivatedEvent(Id, Email));
    }
 
    public void Reactivate()
    {
        if (IsActive)
            throw new DomainException(CustomerErrors.AlreadyActive());
 
        IsActive = true;
        Touch();
 
        Raise(new CustomerReactivatedEvent(Id, Email));
    }
 
    // ─── Guard: used by Order module to verify customer can order ─
    public void EnsureCanPlaceOrder()
    {
        if (!IsActive)
            throw new DomainException(CustomerErrors.InactiveCustomerCannotOrder(Id));
    }
 
    // ─── Validation Helpers ─────────────────────────────────
    private static void ValidateName(string name, string field)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException(CustomerErrors.InvalidName(field));
    }
 
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email))
            throw new DomainException(CustomerErrors.InvalidEmail(email ?? "empty"));
    }
 
    private static void ValidatePhone(string phone)
    {
        if (!PhonePattern.IsMatch(phone))
            throw new DomainException(CustomerErrors.InvalidPhone(phone));
    }
}