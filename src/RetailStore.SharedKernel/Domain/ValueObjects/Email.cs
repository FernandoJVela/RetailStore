using System.Text.RegularExpressions;

namespace RetailStore.SharedKernel.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(new 
                DomainError(
                    "INVALID_EMAIL", 
                    "Email cannot be empty.", 
                    DomainErrorType.BusinessRule)
            );
        if (!Pattern.IsMatch(value))
            throw new DomainException(new 
                DomainError(
                    "INVALID_EMAIL", 
                    "Email is not valid.", 
                    DomainErrorType.BusinessRule)
            );
        Value = value.ToLowerInvariant().Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    { yield return Value; }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}
