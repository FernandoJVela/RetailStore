using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Application;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly RetailStoreDbContext _context;

    public UserRepository(RetailStoreDbContext context)
        => _context = context;

    public async Task<User?> GetByIdAsync(
        Guid id, CancellationToken ct)
        => await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(
        Email email, CancellationToken ct)
        => await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == new Email(email), ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken ct)
        => await _context.Set<User>().ToListAsync(ct);

    public async Task AddAsync(User entity, CancellationToken ct)
        => await _context.Set<User>().AddAsync(entity, ct);

    public void Update(User entity)
        => _context.Set<User>().Update(entity);

    public void Remove(User entity)
        => _context.Set<User>().Remove(entity);

    public async Task<bool> ExistsWithEmailAsync(Email email, CancellationToken ct) 
        => await _context.Set<User>().AnyAsync(u => u.Email == new Email(email), ct);

    public async Task<bool> ExistsWithUsernameAsync(string username, CancellationToken ct)
        => await _context.Set<User>().AnyAsync(u => u.Username == username, ct);
}
