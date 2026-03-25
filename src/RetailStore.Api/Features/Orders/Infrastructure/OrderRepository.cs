using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Orders.Infrastructure;
 
public sealed class OrderRepository : IRepository<Order>
{
    private readonly RetailStoreDbContext _db;
    public OrderRepository(RetailStoreDbContext db) => _db = db;
 
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Order>()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken ct)
        => await _db.Set<Order>().ToListAsync(ct);
 
    public async Task AddAsync(Order entity, CancellationToken ct = default)
        => await _db.Set<Order>().AddAsync(entity, ct);
 
    public void Update(Order entity)
        => _db.Set<Order>().Update(entity);
 
    public void Remove(Order entity)
        => _db.Set<Order>().Remove(entity);
}
