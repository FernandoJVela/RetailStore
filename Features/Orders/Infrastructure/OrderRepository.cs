using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Orders.Infrastructure;

public class OrderRepository : IRepository<Order>
{
    private readonly RetailStoreDbContext _context;

    public OrderRepository(RetailStoreDbContext context)
        => _context = context;

    public async Task<Order?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken ct)
        => await _context.Set<Order>().ToListAsync(ct);

    public async Task AddAsync(Order entity, CancellationToken ct)
        => await _context.Set<Order>().AddAsync(entity, ct);

    public void Update(Order entity)
        => _context.Set<Order>().Update(entity);

    public void Remove(Order entity)
        => _context.Set<Order>().Remove(entity);
}
