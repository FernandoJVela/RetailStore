using RetailStore.Api.Features.Inventory.Domain;

namespace RetailStore.Tests.TestHelpers.Builders;

public class InventoryItemBuilder
{
    private Guid _productId       = Guid.NewGuid();
    private int  _quantity        = 50;
    private int  _threshold       = 10;
    private int  _reserved        = 0;

    public InventoryItemBuilder ForProduct(Guid productId) { _productId = productId; return this; }
    public InventoryItemBuilder WithQuantity(int quantity) { _quantity  = quantity;  return this; }
    public InventoryItemBuilder WithThreshold(int t)       { _threshold = t;         return this; }
    public InventoryItemBuilder WithReserved(int reserved) { _reserved  = reserved;  return this; }

    public InventoryItem Build()
    {
        var item = InventoryItem.Create(_productId, _quantity, _threshold);
        if (_reserved > 0) item.Reserve(_reserved);
        return item;
    }
}
