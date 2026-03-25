namespace RetailStore.SharedKernel.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217
    public static Money Zero => new(0, "USD");
    private Money() { Currency = string.Empty; }    // For EF Core, not for public use. 
                                                    // Use the factory method instead.
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException(new DomainError(
                "INVALID_CURRENCY", "Currency must be 3-letter ISO 4217.",
                DomainErrorType.Validation));
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other) 
    { 
        EnsureSame(other); 
        return new(Amount + other.Amount, Currency); 
    }
    public Money Subtract(Money other) 
    { 
        EnsureSame(other); 
        return new(Amount - other.Amount, Currency); 
    }
    public Money Multiply(int factor) => 
        new(Amount * factor, Currency);

    private void EnsureSame(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException(new DomainError(
                "CURRENCY_MISMATCH", $"Cannot operate on {Currency} and {other.Currency}.",
                DomainErrorType.BusinessRule));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    { yield return Amount; yield return Currency; }
}
