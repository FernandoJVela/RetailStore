using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Shipping.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record ShipmentDto(
    Guid Id, Guid OrderId, Guid CustomerId,
    string Status, string? Carrier, string? TrackingNumber,
    decimal ShippingCost, string CostCurrency,
    int ItemCount, DateTime? ShippedAt, DateTime? DeliveredAt,
    DateTime CreatedAt);
 
public sealed record ShipmentDetailDto(
    Guid Id, Guid OrderId, Guid CustomerId,
    string Status, string? Carrier, string? TrackingNumber,
    DateTime? EstimatedDelivery,
    string Street, string City, string? State, string? ZipCode, string Country,
    decimal ShippingCost, string CostCurrency, decimal? TotalWeightKg,
    DateTime? ShippedAt, DateTime? DeliveredAt, string? Notes,
    List<ShipmentItemDto> Items,
    DateTime CreatedAt, DateTime? UpdatedAt);
 
public sealed record ShipmentItemDto(
    Guid Id, Guid ProductId, string ProductName,
    int Quantity, decimal? WeightKg);
 
// ═══════════════════════════════════════════════════════════
// GET ALL SHIPMENTS
// ═══════════════════════════════════════════════════════════
public sealed record GetShipmentsQuery(
    string? Status = null, Guid? CustomerId = null
) : IQuery<List<ShipmentDto>>;
 
public sealed class GetShipmentsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetShipmentsQuery, List<ShipmentDto>>
{
    public async Task<List<ShipmentDto>> Handle(GetShipmentsQuery query, CancellationToken ct)
    {
        var q = db.Set<Shipment>().AsNoTracking().Include(s => s.Items).AsQueryable();
 
        if (!string.IsNullOrEmpty(query.Status)
            && Enum.TryParse<ShipmentStatus>(query.Status, true, out var status))
            q = q.Where(s => s.Status == status);
 
        if (query.CustomerId.HasValue)
            q = q.Where(s => s.CustomerId == query.CustomerId.Value);
 
        var shipments = await q.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
 
        return shipments.Select(s => new ShipmentDto(
            s.Id, s.OrderId, s.CustomerId,
            s.Status.ToString(), s.Carrier, s.TrackingNumber,
            s.ShippingCost, s.CostCurrency,
            s.Items.Count, s.ShippedAt, s.DeliveredAt,
            s.CreatedAt)).ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET SHIPMENT BY ID (with items)
// ═══════════════════════════════════════════════════════════
public sealed record GetShipmentByIdQuery(Guid Id) : IQuery<ShipmentDetailDto>;
 
public sealed class GetShipmentByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetShipmentByIdQuery, ShipmentDetailDto>
{
    public async Task<ShipmentDetailDto> Handle(GetShipmentByIdQuery query, CancellationToken ct)
    {
        var s = await db.Set<Shipment>()
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == query.Id, ct)
            ?? throw new DomainException(ShippingErrors.NotFound(query.Id));
 
        return new ShipmentDetailDto(
            s.Id, s.OrderId, s.CustomerId,
            s.Status.ToString(), s.Carrier, s.TrackingNumber,
            s.EstimatedDelivery,
            s.Street, s.City, s.State, s.ZipCode, s.Country,
            s.ShippingCost, s.CostCurrency, s.TotalWeightKg,
            s.ShippedAt, s.DeliveredAt, s.Notes,
            s.Items.Select(i => new ShipmentItemDto(
                i.Id, i.ProductId, i.ProductName,
                i.Quantity, i.WeightKg)).ToList(),
            s.CreatedAt, s.UpdatedAt);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET SHIPMENT BY ORDER
// ═══════════════════════════════════════════════════════════
public sealed record GetShipmentByOrderQuery(Guid OrderId) : IQuery<ShipmentDetailDto>;
 
public sealed class GetShipmentByOrderHandler(RetailStoreDbContext db)
    : IRequestHandler<GetShipmentByOrderQuery, ShipmentDetailDto>
{
    public async Task<ShipmentDetailDto> Handle(GetShipmentByOrderQuery query, CancellationToken ct)
    {
        var s = await db.Set<Shipment>()
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.OrderId == query.OrderId, ct)
            ?? throw new DomainException(ShippingErrors.NotFoundByOrder(query.OrderId));
 
        return new ShipmentDetailDto(
            s.Id, s.OrderId, s.CustomerId,
            s.Status.ToString(), s.Carrier, s.TrackingNumber,
            s.EstimatedDelivery,
            s.Street, s.City, s.State, s.ZipCode, s.Country,
            s.ShippingCost, s.CostCurrency, s.TotalWeightKg,
            s.ShippedAt, s.DeliveredAt, s.Notes,
            s.Items.Select(i => new ShipmentItemDto(
                i.Id, i.ProductId, i.ProductName,
                i.Quantity, i.WeightKg)).ToList(),
            s.CreatedAt, s.UpdatedAt);
    }
}
 
// ═══════════════════════════════════════════════════════════
// TRACK SHIPMENT (by tracking number - public)
// ═══════════════════════════════════════════════════════════
public sealed record TrackShipmentQuery(string TrackingNumber) : IQuery<ShipmentTrackingDto>;
 
public sealed record ShipmentTrackingDto(
    string TrackingNumber, string Status, string Carrier,
    DateTime? EstimatedDelivery, DateTime? ShippedAt, DateTime? DeliveredAt,
    string City, string Country);
 
public sealed class TrackShipmentHandler(RetailStoreDbContext db)
    : IRequestHandler<TrackShipmentQuery, ShipmentTrackingDto>
{
    public async Task<ShipmentTrackingDto> Handle(TrackShipmentQuery query, CancellationToken ct)
    {
        var s = await db.Set<Shipment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TrackingNumber == query.TrackingNumber, ct)
            ?? throw new DomainException(ShippingErrors.NotFoundByTracking(query.TrackingNumber));
 
        return new ShipmentTrackingDto(
            s.TrackingNumber!, s.Status.ToString(), s.Carrier ?? "Unknown",
            s.EstimatedDelivery, s.ShippedAt, s.DeliveredAt,
            s.City, s.Country);
    }
}