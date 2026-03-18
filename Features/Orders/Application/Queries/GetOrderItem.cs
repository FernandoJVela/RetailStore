using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
using RetailStore.SharedKernel.Domain.Enums;

namespace RetailStore.Api.Features.Orders.Application.Queries;

public sealed record GetOrderItemByIdQuery(Guid Id) : IQuery<OrderItemDto>;

public sealed record OrderItemDto(
    Guid ProductId, int Quantity);

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
            .Where(p => p.Id == query.Id)
            .Select(p => new OrderItemDto(
                p.ProductId, p.Quantity))
            .FirstOrDefaultAsync(ct);

        // Throws DomainException with 404 mapping automatically
        if (orderItem is null)
            throw new DomainException(OrderItemErrors.OrderItemNotFound(query.Id));

        return orderItem;
    }
}