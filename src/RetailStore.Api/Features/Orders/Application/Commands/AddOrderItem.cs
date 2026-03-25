using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Application.Commands;

// ─── Command ───────────────────────────────────────────────
public sealed record AddOrderItemCommand(
    Guid OrderId, Guid ProductId, int Quantity
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "orders:write";
}

// ─── Validator ─────────────────────────────────────────────
public sealed class AddOrderItemValidator
    : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemValidator()
    {        
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Select a valid order Id.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Select a valid product Id.");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("You must select at least one item.");
    }
}

// ─── Handler ──────────────────────────────────────────────
public sealed class AddOrderItemHandler
    : IRequestHandler<AddOrderItemCommand, Guid>
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Product> _products;

    public AddOrderItemHandler(IRepository<Order> orders, 
        IRepository<Product> products) 
        {
            _orders = orders;
            _products = products;
        }

    public async Task<Guid> Handle(
        AddOrderItemCommand cmd, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(cmd.ProductId);

        if(product == null)
            throw new DomainException(ProductErrors.NotFound(cmd.ProductId));

        var order = await _orders.GetByIdAsync(cmd.OrderId);

        if (order == null)
            throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));

        order.AddItem(
            cmd.ProductId, 
            cmd.Quantity,
            product.Price);

        _orders.Update(order);

        return order.Id;
    }
}
