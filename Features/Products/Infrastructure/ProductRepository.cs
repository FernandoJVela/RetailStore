using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Products.Infrastructure;

public class ProductRepository : IRepository<Product>
{
    private readonly RetailStoreDbContext _context;

    public ProductRepository(RetailStoreDbContext context)
        => _context = context;

    public async Task<Product?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken ct)
        => await _context.Set<Product>().ToListAsync(ct);

    public async Task AddAsync(Product entity, CancellationToken ct)
        => await _context.Set<Product>().AddAsync(entity, ct);

    public void Update(Product entity)
        => _context.Set<Product>().Update(entity);

    public void Remove(Product entity)
        => _context.Set<Product>().Remove(entity);
}
