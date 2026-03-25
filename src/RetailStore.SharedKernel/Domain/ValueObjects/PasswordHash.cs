using System.Security.Cryptography;

namespace RetailStore.SharedKernel.Domain.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Hash { get; }
    public string Salt { get; }

    private PasswordHash(string hash, string salt)
    { Hash = hash; Salt = salt; }

    /// <summary>
    /// Creates a new hash from a plain-text password.
    /// Uses PBKDF2 with SHA-256, 100k iterations.
    /// </summary>
    public static PasswordHash Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 8)
            throw new DomainException(new DomainError(
                "PASSWORD_WEAK", 
                "Password must be at least 8 characters long.", 
                DomainErrorType.BusinessRule
            ));

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword, saltBytes, Iterations,
            HashAlgorithmName.SHA256, HashSize);

        return new PasswordHash(
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// Reconstitutes from stored hash+salt (EF Core).
    /// </summary>
    public static PasswordHash FromStored(string hash, string salt)
        => new(hash, salt);

    public bool Verify(string plainPassword)
    {
        var saltBytes = Convert.FromBase64String(Salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword, saltBytes, Iterations,
            HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(
            hashBytes, Convert.FromBase64String(Hash));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    { yield return Hash; yield return Salt; }
}