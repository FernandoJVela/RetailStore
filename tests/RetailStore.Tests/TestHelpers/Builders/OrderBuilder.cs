using RetailStore.Api.Features.Orders.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Tests.TestHelpers.Builders;

public class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();

    public OrderBuilder WithCustomerId(Guid customerId) { _customerId = customerId; return this; }

    public Order Build() => Order.Create(_customerId);

    public Order BuildWithItem(Guid? productId = null, int quantity = 1, decimal price = 10m)
    {
        var order = Build();
        order.AddItem(productId ?? Guid.NewGuid(), quantity, new Money(price, "USD"));
        return order;
    }

    public Order BuildConfirmed()
    {
        var order = BuildWithItem();
        order.Confirm();
        return order;
    }
}
