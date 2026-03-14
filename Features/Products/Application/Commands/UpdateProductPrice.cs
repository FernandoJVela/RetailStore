using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Products.Application.Commands;

// ─── Command ───────────────────────────────────────────────
public sealed record UpdateProductPriceCommand(
    Guid ProductId, Money Price
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "users:manage";
}

// ─── Validator ─────────────────────────────────────────────
public sealed class UpdateProductPriceValidator
    : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
        RuleFor(x => x.Price.Amount).GreaterThan(0);
    }
}

// ─── Handler ──────────────────────────────────────────────
public sealed class UpdateProductPriceHandler
    : IRequestHandler<UpdateProductPriceCommand, Unit>
{
    private readonly IRepository<Product> _products;

    public UpdateProductPriceHandler(IRepository<Product> products) => 
        _products = products;

    public async Task<Unit> Handle(
        UpdateProductPriceCommand cmd, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(cmd.ProductId, ct);

        if (product is null)
            throw new DomainException(ProductErrors.NotFound(cmd.ProductId));

        product.UpdatePrice(cmd.Price);

        return Unit.Value;
    }
}
