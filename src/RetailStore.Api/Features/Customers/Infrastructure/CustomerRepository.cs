using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Customers.Application;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Customers.Infrastructure;
 
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly RetailStoreDbContext _db;
 
    public CustomerRepository(RetailStoreDbContext db) => _db = db;

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Customer>().ToListAsync(ct);
 
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == id, ct);
 
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant().Trim(), ct);
 
    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        => await _db.Set<Customer>()
            .AnyAsync(c => c.Email == email.ToLowerInvariant().Trim(), ct);
 
    public async Task AddAsync(Customer entity, CancellationToken ct = default)
        => await _db.Set<Customer>().AddAsync(entity, ct);
 
    public void Update(Customer entity)
        => _db.Set<Customer>().Update(entity);
 
    public void Remove(Customer entity)
        => _db.Set<Customer>().Remove(entity);
}