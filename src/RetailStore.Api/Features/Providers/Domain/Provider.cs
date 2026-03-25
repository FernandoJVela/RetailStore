using System.Text.Json;
using System.Text.RegularExpressions;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Providers.Domain;
 
public sealed class Provider : AggregateRoot
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);
 
    public string CompanyName { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
 
    // Stored as JSON array in DB: ["guid1","guid2"]
    public string ProductIds { get; private set; } = "[]";
 
    private Provider() { } // EF Core
 
    // ─── Computed (not in DB) ───────────────────────────────
    public List<Guid> ProductIdList =>
        JsonSerializer.Deserialize<List<Guid>>(ProductIds) ?? new();
 
    public int ProductCount => ProductIdList.Count;
 
    // ─── Factory ────────────────────────────────────────────
    public static Provider Register(
        string companyName, string contactName,
        string email, string? phone = null)
    {
        ValidateCompanyName(companyName);
        ValidateContactName(contactName);
        ValidateEmail(email);
        if (phone is not null) ValidatePhone(phone);
 
        var provider = new Provider
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName.Trim(),
            ContactName = contactName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            Phone = phone?.Trim(),
            ProductIds = "[]"
        };
 
        provider.Raise(new ProviderRegisteredEvent(
            provider.Id, provider.CompanyName, provider.Email));
 
        return provider;
    }
 
    // ─── Profile Updates ────────────────────────────────────
    public void UpdateContactInfo(string companyName, string contactName, string? phone = null)
    {
        ValidateCompanyName(companyName);
        ValidateContactName(contactName);
        if (phone is not null) ValidatePhone(phone);
 
        CompanyName = companyName.Trim();
        ContactName = contactName.Trim();
        Phone = phone?.Trim();
        Touch();
 
        Raise(new ProviderUpdatedEvent(Id, CompanyName, ContactName));
    }
 
    public void ChangeEmail(string newEmail)
    {
        ValidateEmail(newEmail);
        var normalized = newEmail.ToLowerInvariant().Trim();
        if (normalized == Email) return;
 
        var oldEmail = Email;
        Email = normalized;
        Touch();
 
        Raise(new ProviderEmailChangedEvent(Id, oldEmail, Email));
    }
 
    // ─── Product Association ────────────────────────────────
    public void AssociateProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new DomainException(ProviderErrors.InvalidProductId());
 
        var ids = ProductIdList;
        if (ids.Contains(productId))
            throw new DomainException(ProviderErrors.ProductAlreadyAssociated(productId));
 
        ids.Add(productId);
        ProductIds = JsonSerializer.Serialize(ids);
        Touch();
 
        Raise(new ProductAssociatedEvent(Id, productId, CompanyName));
    }
 
    public void DissociateProduct(Guid productId)
    {
        var ids = ProductIdList;
        if (!ids.Contains(productId))
            throw new DomainException(ProviderErrors.ProductNotAssociated(productId));
 
        ids.Remove(productId);
        ProductIds = JsonSerializer.Serialize(ids);
        Touch();
 
        Raise(new ProductDissociatedEvent(Id, productId, CompanyName));
    }
 
    public bool SuppliesProduct(Guid productId) => ProductIdList.Contains(productId);
 
    // ─── Lifecycle ──────────────────────────────────────────
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(ProviderErrors.AlreadyDeactivated());
        IsActive = false;
        Touch();
        Raise(new ProviderDeactivatedEvent(Id, CompanyName));
    }
 
    public void Reactivate()
    {
        if (IsActive)
            throw new DomainException(ProviderErrors.AlreadyActive());
        IsActive = true;
        Touch();
        Raise(new ProviderReactivatedEvent(Id, CompanyName));
    }
 
    // ─── Validation ─────────────────────────────────────────
    private static void ValidateCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException(ProviderErrors.InvalidCompanyName());
    }
 
    private static void ValidateContactName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException(ProviderErrors.InvalidContactName());
    }
 
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email))
            throw new DomainException(ProviderErrors.InvalidEmail(email ?? "empty"));
    }
 
    private static void ValidatePhone(string phone)
    {
        if (!PhonePattern.IsMatch(phone))
            throw new DomainException(ProviderErrors.InvalidPhone(phone));
    }
}