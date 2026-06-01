using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.TestHelpers.Builders;

public class ProductBuilder
{
    private string _name = "Test Product";
    private string _sku = "TST-001";
    private Money _price = new(9.99m, "USD");
    private string _category = "General";
    private string? _description = null;

    public ProductBuilder WithName(string name) { _name = name; return this; }
    public ProductBuilder WithSku(string sku) { _sku = sku; return this; }
    public ProductBuilder WithPrice(decimal amount, string currency = "USD") { _price = new Money(amount, currency); return this; }
    public ProductBuilder WithCategory(string category) { _category = category; return this; }
    public ProductBuilder WithDescription(string description) { _description = description; return this; }

    public Product Build() => Product.Create(_name, _sku, _price, _category, _description);

    public Product BuildInactive()
    {
        var product = Build();
        product.Deactivate();
        return product;
    }
}
