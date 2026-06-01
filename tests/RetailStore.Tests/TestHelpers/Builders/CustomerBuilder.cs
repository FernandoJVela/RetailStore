using RetailStore.Api.Features.Customers.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.TestHelpers.Builders;

public class CustomerBuilder
{
    private string _firstName  = "John";
    private string _lastName   = "Doe";
    private string _email      = $"test-{Guid.NewGuid():N}@example.com";
    private string? _phone     = null;
    private bool _active       = true;
    private Address? _address  = null;

    public CustomerBuilder WithFirstName(string first)  { _firstName = first; return this; }
    public CustomerBuilder WithLastName(string last)    { _lastName  = last;  return this; }
    public CustomerBuilder WithEmail(string email)      { _email     = email; return this; }
    public CustomerBuilder WithPhone(string phone)      { _phone     = phone; return this; }
    public CustomerBuilder Inactive()                   { _active    = false; return this; }
    public CustomerBuilder WithAddress(string street = "123 Main St",
        string city = "Springfield", string state = "IL",
        string zip  = "62701",       string country = "US")
    {
        _address = new Address(street, city, state, zip, country);
        return this;
    }

    public Customer Build()
    {
        var customer = Customer.Register(_firstName, _lastName, _email, _phone);
        if (_address is not null) customer.UpdateShippingAddress(_address);
        if (!_active) customer.Deactivate();
        return customer;
    }
}
