using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Inventory.Application;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Application.Commands;

public sealed record AddOrderItemCommand(
    Guid OrderId, Guid ProductId, int Quantity
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Adding the following item: {ProductId}";
}

public sealed class AddOrderItemValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class AddOrderItemHandler : IRequestHandler<AddOrderItemCommand, Unit>
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Product> _products;
    private readonly IInventoryRepository _inventory;

    public AddOrderItemHandler(
        IRepository<Order> orders,
        IRepository<Product> products,
        IInventoryRepository inventory)
    {
        _orders = orders;
        _products = products;
        _inventory = inventory;
    }

    public async Task<Unit> Handle(AddOrderItemCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));

        var product = await _products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));

        if (!product.IsActive)
            throw new DomainException(ProductErrors.AlreadyDeactivated());

        var inventoryItem = await _inventory.GetByProductIdAsync(cmd.ProductId, ct);
        if (inventoryItem is not null && inventoryItem.AvailableQuantity < cmd.Quantity)
            throw new DomainException(
                InventoryErrors.InsufficientStock(
                    cmd.ProductId, inventoryItem.AvailableQuantity, cmd.Quantity));

        order.AddItem(product.Id, cmd.Quantity, product.Price);
        return Unit.Value;
    }
}
