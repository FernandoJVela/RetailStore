using FluentValidation;
using MediatR;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Orders.Application.Commands;
 
// ─── Confirm ────────────────────────────────────────────────
public sealed record ConfirmOrderCommand(
    Guid OrderId
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Confirming order";
}
 
public sealed class ConfirmOrderHandler(IRepository<Order> orders) : IRequestHandler<ConfirmOrderCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.Confirm();
        return Unit.Value;
    }
}
 
// ─── Complete ───────────────────────────────────────────────
public sealed record CompleteOrderCommand(
    Guid OrderId
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Completing order";
}
 
public sealed class CompleteOrderHandler(IRepository<Order> orders) : IRequestHandler<CompleteOrderCommand, Unit>
{
    public async Task<Unit> Handle(CompleteOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.Complete();
        return Unit.Value;
    }
}
 
// ─── Cancel ─────────────────────────────────────────────────
public sealed record CancelOrderCommand(
    Guid OrderId, 
    string Reason
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Cancelling order: {Reason}";
}
 
public sealed class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
 
public sealed class CancelOrderHandler(IRepository<Order> orders) : IRequestHandler<CancelOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.Cancel(cmd.Reason);
        return Unit.Value;
    }
}
 
// ─── Mark Shipped (triggered by ShipmentShipped event) ──────
public sealed record MarkOrderShippedCommand(
    Guid OrderId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "orders:write";
}

public sealed class MarkOrderShippedHandler(IRepository<Order> orders)
    : IRequestHandler<MarkOrderShippedCommand, Unit>
{
    public async Task<Unit> Handle(MarkOrderShippedCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.MarkShipped();
        return Unit.Value;
    }
}

// ─── Mark Delivered (triggered by ShipmentDelivered event) ──
public sealed record MarkOrderDeliveredCommand(
    Guid OrderId
) : ICommand, IRequirePermission
{
    public string RequiredPermission => "orders:write";
}

public sealed class MarkOrderDeliveredHandler(IRepository<Order> orders)
    : IRequestHandler<MarkOrderDeliveredCommand, Unit>
{
    public async Task<Unit> Handle(MarkOrderDeliveredCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.MarkDelivered();
        return Unit.Value;
    }
}

// ─── Remove Item ────────────────────────────────────────────
public sealed record RemoveOrderItemCommand(
    Guid OrderId, 
    Guid ProductId
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "orders:write";
    public string AuditModule => "Orders";
    public string? AuditDescription => $"Removing item {ProductId}";
}
 
public sealed class RemoveOrderItemHandler(IRepository<Order> orders) : IRequestHandler<RemoveOrderItemCommand, Unit>
{
    public async Task<Unit> Handle(RemoveOrderItemCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
        order.RemoveItem(cmd.ProductId);
        return Unit.Value;
    }
}