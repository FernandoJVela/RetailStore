using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Orders.Application.Queries;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Application.Commands;

// ─── Command ───────────────────────────────────────────────
public sealed record CreateOrderCommand(
    Guid CustomerId, DateTime OrderDate, List<OrderItemDto> Items
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "orders:write";
}

// ─── Validator ─────────────────────────────────────────────
public sealed class CreateOrderValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
            // TODO: Add CustomerExists validation
        
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(HaveUniqueProducts).WithMessage("Order contains duplicate products");
        
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("Product ID is required");
                    
                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive")
                    .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item");
            });
    }
    
    private bool HaveUniqueProducts(List<OrderItemDto> items)
    {
        var productIds = items.Select(i => i.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}

// ─── Handler ──────────────────────────────────────────────
public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Product> _products;

    public CreateOrderHandler(IRepository<Order> orders, 
        IRepository<Product> products) 
        {
            _orders = orders;
            _products = products;
        }

    public async Task<Guid> Handle(
        CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(
            cmd.CustomerId, 
            cmd.OrderDate);
        
        foreach (var itemDto in cmd.Items)
        {
            var product = await _products.GetByIdAsync(itemDto.ProductId);

            if (product == null)
                throw new DomainException(ProductErrors.NotFound(itemDto.ProductId));
            
            if (!product.IsActive)
                throw new DomainException(ProductErrors.AlreadyDeactivated(itemDto.ProductId));
            
            // Check inventory
            //TODO: Validate inventory
            
            order.AddItem(
                product.Id,
                itemDto.Quantity,
                product.Price);
        }

        await _orders.AddAsync(order, ct);

        return order.Id;
    }
}
