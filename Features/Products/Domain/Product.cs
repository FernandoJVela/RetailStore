using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Domain;

public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Product() { } // EF Core

    public static Product Create(
        string name, string sku, decimal price,
        string category, string? description = null)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Sku = sku,
            Price = price,
            Category = category,
            Description = description
        };

        product.RaiseDomainEvent(
            new ProductCreatedEvent(product.Id, name, sku, price));
        return product;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be positive.");

        var oldPrice = Price;
        Price = newPrice;
        SetUpdated();
        IncrementVersion();

        RaiseDomainEvent(
            new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
        RaiseDomainEvent(new ProductDeactivatedEvent(Id));
    }
}
