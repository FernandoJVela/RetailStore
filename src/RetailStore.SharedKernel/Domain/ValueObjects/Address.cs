namespace RetailStore.SharedKernel.Domain.ValueObjects;
 
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }
 
    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
        Country = string.Empty;
    }
 
    public Address(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException(new DomainError("CUSTOMER_INVALID_ADDRESS",
                $"Invalid address: Street is required",
                DomainErrorType.Validation));
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(new DomainError("CUSTOMER_INVALID_ADDRESS",
                $"Invalid address: City is required",
                DomainErrorType.Validation));
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException(new DomainError("CUSTOMER_INVALID_ADDRESS",
                $"Invalid address: Country is required",
                DomainErrorType.Validation));
 
        Street = street.Trim();
        City = city.Trim();
        State = state?.Trim() ?? string.Empty;
        ZipCode = zipCode?.Trim() ?? string.Empty;
        Country = country.Trim();
    }
 
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";
 
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
 
    public override string ToString() => FullAddress;
}