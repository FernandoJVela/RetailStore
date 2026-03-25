using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Products.Application.Commands;

// ─── Command ───────────────────────────────────────────────
public sealed record CreateProductCommand(
    string Name, string Sku, decimal Price, string Currency,
    string Category, string? Description = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "products:write";
}

// ─── Validator ─────────────────────────────────────────────
public sealed class CreateProductValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Category).NotEmpty();
    }
}

// ─── Handler ──────────────────────────────────────────────
public sealed class CreateProductHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IRepository<Product> _products;

    public CreateProductHandler(IRepository<Product> products)
        => _products = products;

    public async Task<Guid> Handle(
        CreateProductCommand cmd, CancellationToken ct)
    {
        var price = new Money(cmd.Price, cmd.Currency);
        var product = Product.Create(
            cmd.Name, cmd.Sku, price,
            cmd.Category, cmd.Description);

        await _products.AddAsync(product, ct);

        return product.Id;
    }
}
