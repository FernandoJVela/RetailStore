using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Users.Infrastructure;

public class RoleRepository : IRepository<Role>
{
    private readonly RetailStoreDbContext _context;

    public RoleRepository(RetailStoreDbContext context)
        => _context = context;

    public async Task<Role?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken ct)
        => await _context.Set<Role>().ToListAsync(ct);

    public async Task AddAsync(Role entity, CancellationToken ct)
        => await _context.Set<Role>().AddAsync(entity, ct);

    public void Update(Role entity)
        => _context.Set<Role>().Update(entity);

    public void Remove(Role entity)
        => _context.Set<Role>().Remove(entity);
}
