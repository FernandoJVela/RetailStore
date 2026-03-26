using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain.ValueObjects;
 
namespace RetailStore.Api.Features.Products.Application.Commands;
 
public sealed record CreateProductCommand(
    string Name, string Sku, decimal Price, string Currency,
    string Category, string? Description = null
) : ICommand<Guid>, IRequirePermission, IAuditable
{
    public string RequiredPermission => "products:write";
     public string AuditModule => "Products";
     public string? AuditDescription => $"Creating product {Name} ({Sku})";
}
 
public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}
 
public sealed class CreateProductHandler(IRepository<Product> products)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var price = new Money(cmd.Price, cmd.Currency);
        var product = Product.Create(
            cmd.Name, cmd.Sku, price, cmd.Category, cmd.Description);
 
        await products.AddAsync(product, ct);
        return product.Id;
    }
}
