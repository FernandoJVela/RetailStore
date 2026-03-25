using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Inventory.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record InventoryItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    int ReorderThreshold,
    string StockStatus);
 
public sealed record InventoryDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    int ReorderThreshold,
    string StockStatus,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
 
// ═══════════════════════════════════════════════════════════
// GET ALL INVENTORY (with product info and stock status)
// ═══════════════════════════════════════════════════════════
public sealed record GetInventoryQuery(
    string? StockStatus = null
) : IQuery<List<InventoryItemDto>>;
 
public sealed class GetInventoryHandler(RetailStoreDbContext db)
    : IRequestHandler<GetInventoryQuery, List<InventoryItemDto>>
{
    public async Task<List<InventoryItemDto>> Handle(GetInventoryQuery query, CancellationToken ct)
    {
        var items = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Join(db.Set<Product>().AsNoTracking(),
                i => i.ProductId,
                p => p.Id,
                (i, p) => new InventoryItemDto(
                    i.Id,
                    i.ProductId,
                    p.Name,
                    p.Sku,
                    i.QuantityOnHand,
                    i.ReservedQuantity,
                    i.QuantityOnHand - i.ReservedQuantity,
                    i.ReorderThreshold,
                    (i.QuantityOnHand - i.ReservedQuantity) <= 0
                        ? "OutOfStock"
                        : i.QuantityOnHand <= i.ReorderThreshold
                            ? "LowStock"
                            : "InStock"))
            .ToListAsync(ct);
 
        if (!string.IsNullOrEmpty(query.StockStatus))
            items = items.Where(i =>
                i.StockStatus.Equals(query.StockStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();
 
        return items.OrderBy(i => i.StockStatus).ThenBy(i => i.ProductName).ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET INVENTORY BY PRODUCT ID
// ═══════════════════════════════════════════════════════════
public sealed record GetInventoryByProductQuery(Guid ProductId) : IQuery<InventoryDetailDto>;
 
public sealed class GetInventoryByProductHandler(RetailStoreDbContext db)
    : IRequestHandler<GetInventoryByProductQuery, InventoryDetailDto>
{
    public async Task<InventoryDetailDto> Handle(GetInventoryByProductQuery query, CancellationToken ct)
    {
        var result = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(i => i.ProductId == query.ProductId)
            .Join(db.Set<Product>().AsNoTracking(),
                i => i.ProductId,
                p => p.Id,
                (i, p) => new InventoryDetailDto(
                    i.Id,
                    i.ProductId,
                    p.Name,
                    p.Sku,
                    i.QuantityOnHand,
                    i.ReservedQuantity,
                    i.QuantityOnHand - i.ReservedQuantity,
                    i.ReorderThreshold,
                    (i.QuantityOnHand - i.ReservedQuantity) <= 0
                        ? "OutOfStock"
                        : i.QuantityOnHand <= i.ReorderThreshold
                            ? "LowStock"
                            : "InStock",
                    i.CreatedAt,
                    i.UpdatedAt))
            .FirstOrDefaultAsync(ct)
            ?? throw new DomainException(InventoryErrors.NotFoundByProduct(query.ProductId));
 
        return result;
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET LOW STOCK ITEMS (for reorder alerts)
// ═══════════════════════════════════════════════════════════
public sealed record GetLowStockQuery() : IQuery<List<InventoryItemDto>>;
 
public sealed class GetLowStockHandler(RetailStoreDbContext db)
    : IRequestHandler<GetLowStockQuery, List<InventoryItemDto>>
{
    public async Task<List<InventoryItemDto>> Handle(GetLowStockQuery query, CancellationToken ct)
    {
        var items = await (
            from i in db.Set<InventoryItem>().AsNoTracking()
            join p in db.Set<Product>().AsNoTracking() on i.ProductId equals p.Id
            where i.QuantityOnHand <= i.ReorderThreshold
            select new InventoryItemDto(
                i.Id,
                i.ProductId,
                p.Name,
                p.Sku,
                i.QuantityOnHand,
                i.ReservedQuantity,
                i.QuantityOnHand - i.ReservedQuantity,
                i.ReorderThreshold,
                (i.QuantityOnHand - i.ReservedQuantity) <= 0
                    ? "OutOfStock"
                    : "LowStock")
        ).ToListAsync(ct);

        return items.OrderBy(i => i.AvailableQuantity).ToList();
    }
}