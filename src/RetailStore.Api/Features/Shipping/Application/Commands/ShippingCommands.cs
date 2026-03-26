using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// CREATE SHIPMENT (from a confirmed order)
// ═══════════════════════════════════════════════════════════
public sealed record CreateShipmentCommand(
    Guid OrderId
) : ICommand<Guid>, IRequirePermission, IAuditable
{
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Creating new shipping";
}
 
public sealed class CreateShipmentValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
 
public sealed class CreateShipmentHandler(
    IShipmentRepository shipments,
    RetailStoreDbContext db)
    : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand cmd, CancellationToken ct)
    {
        // Check no duplicate shipment
        if (await shipments.ExistsForOrderAsync(cmd.OrderId, ct))
            throw new DomainException(ShippingErrors.AlreadyExists(cmd.OrderId));
 
        // Load order with items
        var order = await db.Set<Order>()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(cmd.OrderId));
 
        // Load customer for address
        var customer = await db.Set<Customer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == order.CustomerId, ct)
            ?? throw new DomainException(CustomerErrors.NotFound(order.CustomerId));
 
        // Use customer shipping address or fallback
        var street = customer.ShippingStreet ?? "Address not provided";
        var city = customer.ShippingCity ?? "Unknown";
        var country = customer.ShippingCountry ?? "Unknown";
 
        var shipment = Shipment.Create(
            order.Id, customer.Id,
            street, city, country,
            customer.ShippingState, customer.ShippingZipCode);
 
        // Add items from order line items
        foreach (var lineItem in order.Items)
        {
            var product = await db.Set<Product>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == lineItem.ProductId, ct);
 
            shipment.AddItem(
                lineItem.ProductId,
                product?.Name ?? $"Product {lineItem.ProductId}",
                lineItem.Quantity);
        }
 
        await shipments.AddAsync(shipment, ct);
        return shipment.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// ASSIGN CARRIER
// ═══════════════════════════════════════════════════════════
public sealed record AssignCarrierCommand(
    Guid ShipmentId, string Carrier,
    string TrackingNumber, DateTime? EstimatedDelivery = null
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Assigning carrier to shipping: {Carrier}";
}
 
public sealed class AssignCarrierValidator : AbstractValidator<AssignCarrierCommand>
{
    public AssignCarrierValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Carrier).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(100);
    }
}
 
public sealed class AssignCarrierHandler(IShipmentRepository shipments)
    : IRequestHandler<AssignCarrierCommand, Unit>
{
    public async Task<Unit> Handle(AssignCarrierCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.AssignCarrier(cmd.Carrier, cmd.TrackingNumber, cmd.EstimatedDelivery);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// SET SHIPPING COST
// ═══════════════════════════════════════════════════════════
public sealed record SetShippingCostCommand(
    Guid ShipmentId, decimal Cost, string Currency = "USD"
) : ICommand, IRequirePermission, IAuditable
{
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Setting shipping cost {Cost} {Currency}";
}
 
public sealed class SetShippingCostHandler(IShipmentRepository shipments)
    : IRequestHandler<SetShippingCostCommand, Unit>
{
    public async Task<Unit> Handle(SetShippingCostCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.SetShippingCost(cmd.Cost, cmd.Currency);
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// STATUS TRANSITIONS
// ═══════════════════════════════════════════════════════════
public sealed record MarkShippedCommand(
    Guid ShipmentId
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Marking shipping";
}
 
public sealed class MarkShippedHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkShippedCommand, Unit>
{
    public async Task<Unit> Handle(MarkShippedCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.MarkShipped();
        return Unit.Value;
    }
}
 
public sealed record MarkInTransitCommand(Guid ShipmentId) : ICommand, IRequirePermission
{ public string RequiredPermission => "shipping:write"; }
 
public sealed class MarkInTransitHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkInTransitCommand, Unit>
{
    public async Task<Unit> Handle(MarkInTransitCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.MarkInTransit();
        return Unit.Value;
    }
}
 
public sealed record MarkDeliveredCommand(
    Guid ShipmentId
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Marking shipping as delivered";
}
 
public sealed class MarkDeliveredHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkDeliveredCommand, Unit>
{
    public async Task<Unit> Handle(MarkDeliveredCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.MarkDelivered();
        return Unit.Value;
    }
}
 
public sealed record MarkFailedCommand(
    Guid ShipmentId, string Reason
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Marking shipping as failed: {Reason}";
}
 
public sealed class MarkFailedHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkFailedCommand, Unit>
{
    public async Task<Unit> Handle(MarkFailedCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.MarkFailed(cmd.Reason);
        return Unit.Value;
    }
}
 
public sealed record MarkReturnedCommand(
    Guid ShipmentId, string Reason
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Marking shipping as returned: {Reason}";
}
 
public sealed class MarkReturnedHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkReturnedCommand, Unit>
{
    public async Task<Unit> Handle(MarkReturnedCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.MarkReturned(cmd.Reason);
        return Unit.Value;
    }
}
 
public sealed record CancelShipmentCommand(
    Guid ShipmentId, string Reason
) : ICommand, IRequirePermission, IAuditable
{ 
    public string RequiredPermission => "shipping:write";
    public string AuditModule => "Shipping";
    public string? AuditDescription => $"Cancelling shipping: {Reason}";
}
 
public sealed class CancelShipmentHandler(IShipmentRepository shipments)
    : IRequestHandler<CancelShipmentCommand, Unit>
{
    public async Task<Unit> Handle(CancelShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(cmd.ShipmentId, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(cmd.ShipmentId));
        shipment.Cancel(cmd.Reason);
        return Unit.Value;
    }
}