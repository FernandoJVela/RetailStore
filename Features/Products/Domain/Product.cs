using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Domain;

public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Product() { }  // EF Core

    /// <summary>
    /// Factory method. Creates a valid Product or throws DomainException.
    /// Domain invariants are enforced HERE, not in the handler.
    /// </summary>
    public static Product Create(
        string name, string sku, decimal price,
        string category, string? description = null)
    {
        if (price <= 0)
            throw new DomainException(ProductErrors.InvalidPrice());

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name, Sku = sku, Price = price,
            Category = category, Description = description
        };

        product.Raise(new ProductCreatedEvent(
            product.Id, name, sku, price));
        return product;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException(ProductErrors.InvalidPrice());

        var oldPrice = Price;
        Price = newPrice;
        Touch();
        IncrementVersion();
        Raise(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(
                ProductErrors.AlreadyDeactivated(Id));

        IsActive = false;
        Touch();
        Raise(new ProductDeactivatedEvent(Id));
    }
}
