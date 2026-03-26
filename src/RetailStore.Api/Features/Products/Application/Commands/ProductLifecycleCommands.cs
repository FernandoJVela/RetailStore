using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Products.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// UPDATE PRODUCT DETAILS (name, category, description)
// ═══════════════════════════════════════════════════════════
public sealed record UpdateProductDetailsCommand(
    Guid ProductId, string Name, string Category, string? Description = null
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "products:write";
    public string AuditModule => "Products";
    public string? AuditDescription => $"Updating product {Name}";
}
 
public sealed class UpdateProductDetailsValidator : AbstractValidator<UpdateProductDetailsCommand>
{
    public UpdateProductDetailsValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}
 
public sealed class UpdateProductDetailsHandler(IRepository<Product> products)
    : IRequestHandler<UpdateProductDetailsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductDetailsCommand cmd, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        product.UpdateDetails(cmd.Name, cmd.Category, cmd.Description);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// DEACTIVATE PRODUCT (FIXED: no AddAsync on existing entity)
// ═══════════════════════════════════════════════════════════
public sealed record DeactivateProductCommand(
    Guid ProductId
    ) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "products:write";
    public string AuditModule => "Products";
    public string? AuditDescription => $"Deactivating product: {ProductId}";
}
 
public sealed class DeactivateProductValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
 
public sealed class DeactivateProductHandler(IRepository<Product> products)
    : IRequestHandler<DeactivateProductCommand, Unit>
{
    public async Task<Unit> Handle(DeactivateProductCommand cmd, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        product.Deactivate();
        // No AddAsync! The entity is already tracked by EF from GetByIdAsync.
        // UnitOfWorkBehavior calls SaveChanges automatically.
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// REACTIVATE PRODUCT
// ═══════════════════════════════════════════════════════════
public sealed record ReactivateProductCommand(
    Guid ProductId
    ) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "products:write";
    public string AuditModule => "Products";
    public string? AuditDescription => $"Reactivating product: {ProductId}";
}
 
public sealed class ReactivateProductHandler(IRepository<Product> products)
    : IRequestHandler<ReactivateProductCommand, Unit>
{
    public async Task<Unit> Handle(ReactivateProductCommand cmd, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        product.Reactivate();
        return Unit.Value;
    }
}