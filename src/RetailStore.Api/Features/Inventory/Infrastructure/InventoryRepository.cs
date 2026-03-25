using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Inventory.Application;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Inventory.Infrastructure;
 
public sealed class InventoryRepository : IInventoryRepository
{
    private readonly RetailStoreDbContext _db;
    public InventoryRepository(RetailStoreDbContext db) => _db = db;
 
    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.Id == id, ct);
 
    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        => await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.ProductId == productId, ct);
 
    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<InventoryItem>().ToListAsync(ct);
 
    public async Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct = default)
        => await _db.Set<InventoryItem>().AnyAsync(i => i.ProductId == productId, ct);
 
    public async Task AddAsync(InventoryItem entity, CancellationToken ct = default)
        => await _db.Set<InventoryItem>().AddAsync(entity, ct);
 
    public void Update(InventoryItem entity)
        => _db.Set<InventoryItem>().Update(entity);
 
    public void Remove(InventoryItem entity)
        => _db.Set<InventoryItem>().Remove(entity);
}