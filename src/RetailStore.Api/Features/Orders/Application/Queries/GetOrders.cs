using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Orders.Application.Queries;

public sealed record GetOrdersQuery() : IQuery<List<OrderDto>>;

public sealed class GetOrdersHandler
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly RetailStoreDbContext _db;

    public GetOrdersHandler(RetailStoreDbContext db) => _db = db;

    public async Task<List<OrderDto>> Handle(
        GetOrdersQuery query, CancellationToken ct)
    {
        var orders = await _db.Set<Order>()
            .AsNoTracking()
            .Select(p => new OrderDto(
                p.Id, p.CustomerId, p.Status,
                p.OrderDate))
            .ToListAsync(ct);

        if (orders is null)
            throw new DomainException(OrderErrors.OrderNotFound());

        return orders;
    }
}