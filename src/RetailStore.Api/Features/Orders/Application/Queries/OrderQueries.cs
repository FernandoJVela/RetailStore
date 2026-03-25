using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;
 
namespace RetailStore.Api.Features.Orders.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record OrderDto(
    Guid Id, Guid CustomerId, string Status, DateTime OrderDate,
    decimal TotalAmount, int ItemCount,
    DateTime? CompletedAt, DateTime? CancelledAt);
 
public sealed record OrderDetailDto(
    Guid Id, Guid CustomerId, string Status, DateTime OrderDate,
    decimal TotalAmount, DateTime? CompletedAt, DateTime? CancelledAt,
    List<OrderItemDto> Items);
 
public sealed record OrderItemDto(
    Guid Id, Guid ProductId, int Quantity,
    decimal UnitPrice, string Currency, decimal Subtotal);
 
// ─── Get All Orders ─────────────────────────────────────────
public sealed record GetOrdersQuery(string? Status = null) : IQuery<List<OrderDto>>;
 
public sealed class GetOrdersHandler(RetailStoreDbContext db)
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        var q = db.Set<Order>().AsNoTracking().AsQueryable();
 
        if (!string.IsNullOrEmpty(query.Status)
            && Enum.TryParse<OrderStatus>(query.Status, true, out var status))
            q = q.Where(o => o.Status == status);
 
        return await q.OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto(
                o.Id, o.CustomerId, o.Status.ToString(), o.OrderDate,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.Items.Count,
                o.CompletedAt, o.CancelledAt))
            .ToListAsync(ct);
    }
}
 
// ─── Get Order By Id (with items) ───────────────────────────
public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDetailDto>;
 
public sealed class GetOrderByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await db.Set<Order>()
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Id == query.Id)
            .Select(o => new OrderDetailDto(
                o.Id, o.CustomerId, o.Status.ToString(), o.OrderDate,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.CompletedAt, o.CancelledAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.ProductId, i.Quantity,
                    i.UnitPrice, i.UnitPriceCurrency,
                    i.UnitPrice * i.Quantity)).ToList()))
            .FirstOrDefaultAsync(ct)
            ?? throw new DomainException(OrderErrors.OrderNotFound(query.Id));
 
        return order;
    }
}