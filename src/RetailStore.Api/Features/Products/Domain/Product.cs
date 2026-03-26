using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
 
namespace RetailStore.Api.Features.Products.Domain;
 
public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = Money.Zero;
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
 
    private Product() { } // EF Core
 
    // ─── Factory ────────────────────────────────────────────
    public static Product Create(
        string name, string sku, Money price,
        string category, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.InvalidName());
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException(ProductErrors.InvalidSku());
        if (price.Amount <= 0)
            throw new DomainException(ProductErrors.InvalidPrice());
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException(ProductErrors.InvalidCategory());
 
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Price = price,
            Category = category.Trim(),
            Description = description?.Trim()
        };
 
        product.Raise(new ProductCreatedEvent(
            product.Id, product.Name, product.Sku,
            price.Amount, price.Currency));
 
        return product;
    }
 
    // ─── Updates ────────────────────────────────────────────
    public void UpdateDetails(string name, string category, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.InvalidName());
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException(ProductErrors.InvalidCategory());
 
        Name = name.Trim();
        Category = category.Trim();
        Description = description?.Trim();
        Touch();
 
        Raise(new ProductUpdatedEvent(Id, Name, Category));
    }
 
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount <= 0)
            throw new DomainException(ProductErrors.InvalidPrice());
 
        var oldAmount = Price.Amount;
        var oldCurrency = Price.Currency;
        Price = newPrice;
        Touch();
        IncrementVersion();
 
        Raise(new ProductPriceChangedEvent(
            Id, oldAmount, oldCurrency, newPrice.Amount, newPrice.Currency));
    }
 
    // ─── Lifecycle ──────────────────────────────────────────
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(ProductErrors.AlreadyDeactivated());
 
        IsActive = false;
        Touch();
        Raise(new ProductDeactivatedEvent(Id, Name, Sku));
    }
 
    public void Reactivate()
    {
        if (IsActive)
            throw new DomainException(ProductErrors.AlreadyActive());
 
        IsActive = true;
        Touch();
        Raise(new ProductReactivatedEvent(Id, Name, Sku));
    }
}
