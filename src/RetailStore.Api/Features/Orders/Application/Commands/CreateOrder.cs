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

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(
    Guid CustomerId, DateTime? OrderDate, List<CreateOrderItemDto> Items
) : ICommand<Guid>, IRequirePermission, IAuditable
{
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Creating order with {Items.Count} items.";
}

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item.")
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate products.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
        });
    }
}

public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Product> _products;
    private readonly IInventoryRepository _inventory;

    public CreateOrderHandler(
        IRepository<Order> orders,
        IRepository<Product> products,
        IInventoryRepository inventory)
    {
        _orders = orders;
        _products = products;
        _inventory = inventory;
    }

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.OrderDate);

        foreach (var itemDto in cmd.Items)
        {
            var product = await _products.GetByIdAsync(itemDto.ProductId, ct)
                ?? throw new DomainException(ProductErrors.NotFound(itemDto.ProductId));

            if (!product.IsActive)
                throw new DomainException(ProductErrors.AlreadyDeactivated());

            var inventoryItem = await _inventory.GetByProductIdAsync(itemDto.ProductId, ct);
            if (inventoryItem is not null && inventoryItem.AvailableQuantity < itemDto.Quantity)
                throw new DomainException(
                    InventoryErrors.InsufficientStock(
                        itemDto.ProductId, inventoryItem.AvailableQuantity, itemDto.Quantity));

            order.AddItem(product.Id, itemDto.Quantity, product.Price);
        }

        await _orders.AddAsync(order, ct);
        return order.Id;
    }
}
