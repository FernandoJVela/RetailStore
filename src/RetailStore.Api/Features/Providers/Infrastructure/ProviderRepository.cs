using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Providers.Application;
using RetailStore.Api.Features.Providers.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Providers.Infrastructure;
 
public sealed class ProviderRepository : IProviderRepository
{
    private readonly RetailStoreDbContext _db;
    public ProviderRepository(RetailStoreDbContext db) => _db = db;

    public async Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Provider>().ToListAsync(ct);
         
    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Provider>().FirstOrDefaultAsync(p => p.Id == id, ct);
 
    public async Task<Provider?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Set<Provider>()
            .FirstOrDefaultAsync(p => p.Email == email.ToLowerInvariant().Trim(), ct);
 
    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        => await _db.Set<Provider>()
            .AnyAsync(p => p.Email == email.ToLowerInvariant().Trim(), ct);
 
    public async Task<List<Provider>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        // JSON column search — load active providers and filter in memory
        var providers = await _db.Set<Provider>()
            .Where(p => p.IsActive)
            .ToListAsync(ct);
 
        return providers.Where(p => p.SuppliesProduct(productId)).ToList();
    }
 
    public async Task AddAsync(Provider entity, CancellationToken ct = default)
        => await _db.Set<Provider>().AddAsync(entity, ct);
 
    public void Update(Provider entity)
        => _db.Set<Provider>().Update(entity);
 
    public void Remove(Provider entity)
        => _db.Set<Provider>().Remove(entity);
}