using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Products.Application.Commands;

// ─── Command ───────────────────────────────────────────────
public sealed record DeactivateProductCommand(
    Guid ProductId
) : ICommand;

// ─── Validator ─────────────────────────────────────────────
public sealed class DeactivateProductValidator
    : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
    }
}

// ─── Handler ──────────────────────────────────────────────
public sealed class DeactivateProductHandler
    : IRequestHandler<DeactivateProductCommand, Unit>
{
    private readonly IRepository<Product> _products;

    public DeactivateProductHandler(IRepository<Product> products)
        => _products = products;

    public async Task<Unit> Handle(
        DeactivateProductCommand cmd, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(cmd.ProductId, ct);

        if (product is null)
            throw new DomainException(ProductErrors.NotFound(cmd.ProductId));

        product.Deactivate();

        await _products.AddAsync(product, ct);

        return Unit.Value;
    }
}
