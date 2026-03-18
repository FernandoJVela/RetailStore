using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Orders.Infrastructure;

public class OrderItemRepository : IRepository<OrderItem>
{
    private readonly RetailStoreDbContext _context;

    public OrderItemRepository(RetailStoreDbContext context)
        => _context = context;

    public async Task<OrderItem?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _context.Set<OrderItem>()
            .FirstOrDefaultAsync(oi => oi.Id == id, ct);

    public async Task<IReadOnlyList<OrderItem>> GetAllAsync(
        CancellationToken ct)
        => await _context.Set<OrderItem>().ToListAsync(ct);

    public async Task AddAsync(OrderItem entity, CancellationToken ct)
        => await _context.Set<OrderItem>().AddAsync(entity, ct);

    public void Update(OrderItem entity)
        => _context.Set<OrderItem>().Update(entity);

    public void Remove(OrderItem entity)
        => _context.Set<OrderItem>().Remove(entity);
}
