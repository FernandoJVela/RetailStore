using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;

namespace RetailStore.Api.Features.Orders.Application.Queries;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto>;

public sealed record OrderDto(
    Guid Id, Guid CustomerId, OrderStatus Status,
    DateTime OrderDate);

public sealed class GetOrderByIdHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly RetailStoreDbContext _db;

    public GetOrderByIdHandler(RetailStoreDbContext db) => _db = db;

    public async Task<OrderDto> Handle(
        GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await _db.Set<Order>()
            .AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new OrderDto(
                p.Id, p.CustomerId, p.Status,
                p.OrderDate))
            .FirstOrDefaultAsync(ct);

        if (order is null)
            throw new DomainException(OrderErrors.OrderNotFound(query.Id));

        return order;
    }
}