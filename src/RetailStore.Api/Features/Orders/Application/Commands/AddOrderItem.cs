using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Orders.Application.Commands;
 
public sealed record AddOrderItemCommand(
    Guid OrderId, Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "orders:write";
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
 
    public AddOrderItemHandler(IRepository<Order> orders, IRepository<Product> products)
    { _orders = orders; _products = products; }
 
    public async Task<Unit> Handle(AddOrderItemCommand cmd, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
 
        var product = await _products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        if (!product.IsActive)
            throw new DomainException(ProductErrors.AlreadyDeactivated(cmd.ProductId));
 
        order.AddItem(product.Id, cmd.Quantity, product.Price);
        return Unit.Value;
    }
}