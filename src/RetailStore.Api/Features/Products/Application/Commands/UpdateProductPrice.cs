using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;
 
namespace RetailStore.Api.Features.Products.Application.Commands;
 
public sealed record UpdateProductPriceCommand(
    Guid ProductId, decimal Price, string Currency = "USD"
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "products:write";
}
 
public sealed class UpdateProductPriceValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
 
public sealed class UpdateProductPriceHandler(IRepository<Product> products)
    : IRequestHandler<UpdateProductPriceCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductPriceCommand cmd, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        product.UpdatePrice(new Money(cmd.Price, cmd.Currency));
        
        return Unit.Value;
    }
}