using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Inventory.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// CREATE INVENTORY ITEM
// ═══════════════════════════════════════════════════════════
public sealed record CreateInventoryItemCommand(
    Guid ProductId, int InitialQuantity, int ReorderThreshold = 10
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "inventory:write";
}
 
public sealed class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderThreshold).GreaterThanOrEqualTo(0);
    }
}
 
public sealed class CreateInventoryItemHandler(
    IInventoryRepository inventory,
    IRepository<Product> products)
    : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateInventoryItemCommand cmd, CancellationToken ct)
    {
        // Verify product exists
        var product = await products.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(ProductErrors.NotFound(cmd.ProductId));
 
        // Check no duplicate inventory for this product
        if (await inventory.ExistsForProductAsync(cmd.ProductId, ct))
            throw new DomainException(InventoryErrors.AlreadyExists(cmd.ProductId));
 
        var item = InventoryItem.Create(cmd.ProductId, cmd.InitialQuantity, cmd.ReorderThreshold);
        await inventory.AddAsync(item, ct);
        return item.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// ADD STOCK (Replenishment)
// ═══════════════════════════════════════════════════════════
public sealed record AddStockCommand(
    Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class AddStockValidator : AbstractValidator<AddStockCommand>
{
    public AddStockValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive.");
    }
}
 
public sealed class AddStockHandler(IInventoryRepository inventory)
    : IRequestHandler<AddStockCommand, Unit>
{
    public async Task<Unit> Handle(AddStockCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.AddStock(cmd.Quantity);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// REMOVE STOCK (Damage, loss, etc.)
// ═══════════════════════════════════════════════════════════
public sealed record RemoveStockCommand(
    Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class RemoveStockValidator : AbstractValidator<RemoveStockCommand>
{
    public RemoveStockValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
 
public sealed class RemoveStockHandler(IInventoryRepository inventory)
    : IRequestHandler<RemoveStockCommand, Unit>
{
    public async Task<Unit> Handle(RemoveStockCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.RemoveStock(cmd.Quantity);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// RESERVE STOCK (Called when order is placed)
// ═══════════════════════════════════════════════════════════
public sealed record ReserveStockCommand(
    Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class ReserveStockHandler(IInventoryRepository inventory)
    : IRequestHandler<ReserveStockCommand, Unit>
{
    public async Task<Unit> Handle(ReserveStockCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.Reserve(cmd.Quantity);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// RELEASE RESERVATION (Called when order is cancelled)
// ═══════════════════════════════════════════════════════════
public sealed record ReleaseReservationCommand(
    Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class ReleaseReservationHandler(IInventoryRepository inventory)
    : IRequestHandler<ReleaseReservationCommand, Unit>
{
    public async Task<Unit> Handle(ReleaseReservationCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.ReleaseReservation(cmd.Quantity);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// FULFILL RESERVATION (Called when order is shipped/completed)
// ═══════════════════════════════════════════════════════════
public sealed record FulfillReservationCommand(
    Guid ProductId, int Quantity
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class FulfillReservationHandler(IInventoryRepository inventory)
    : IRequestHandler<FulfillReservationCommand, Unit>
{
    public async Task<Unit> Handle(FulfillReservationCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.FulfillReservation(cmd.Quantity);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// ADJUST STOCK (Manual correction with reason)
// ═══════════════════════════════════════════════════════════
public sealed record AdjustStockCommand(
    Guid ProductId, int NewQuantity, string Reason
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
 
public sealed class AdjustStockHandler(IInventoryRepository inventory)
    : IRequestHandler<AdjustStockCommand, Unit>
{
    public async Task<Unit> Handle(AdjustStockCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.AdjustQuantity(cmd.NewQuantity, cmd.Reason);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// UPDATE REORDER THRESHOLD
// ═══════════════════════════════════════════════════════════
public sealed record UpdateReorderThresholdCommand(
    Guid ProductId, int NewThreshold
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "inventory:adjust";
}
 
public sealed class UpdateReorderThresholdValidator : AbstractValidator<UpdateReorderThresholdCommand>
{
    public UpdateReorderThresholdValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewThreshold).GreaterThanOrEqualTo(0);
    }
}
 
public sealed class UpdateReorderThresholdHandler(IInventoryRepository inventory)
    : IRequestHandler<UpdateReorderThresholdCommand, Unit>
{
    public async Task<Unit> Handle(UpdateReorderThresholdCommand cmd, CancellationToken ct)
    {
        var item = await inventory.GetByProductIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(cmd.ProductId));
 
        item.UpdateReorderThreshold(cmd.NewThreshold);
        return Unit.Value;
    }
}