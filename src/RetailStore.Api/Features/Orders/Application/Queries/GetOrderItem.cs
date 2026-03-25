using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Application.Queries;

public sealed record GetOrderItemByIdQuery(Guid Id) : IQuery<OrderItemDto>;

public sealed record OrderItemDto(
    Guid ProductId, string ProductName, int Quantity);

public sealed class GetOrderItemByIdHandler
    : IRequestHandler<GetOrderItemByIdQuery, OrderItemDto>
{
    private readonly RetailStoreDbContext _db;

    public GetOrderItemByIdHandler(RetailStoreDbContext db) => _db = db;

    public async Task<OrderItemDto> Handle(
        GetOrderItemByIdQuery query, CancellationToken ct)
    {
         var orderItem = await _db.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.OrderId == query.Id)
            .Join(
                _db.Set<Product>(),
                oi => oi.ProductId,
                p => p.Id,
                (oi, p) => new OrderItemDto(
                    oi.ProductId,
                    p.Name,
                    oi.Quantity
                )
            )
            .FirstOrDefaultAsync(ct);

        if (orderItem is null)
            throw new DomainException(
                OrderItemErrors.OrderItemNotFound(query.Id));

        return orderItem;
    }
}