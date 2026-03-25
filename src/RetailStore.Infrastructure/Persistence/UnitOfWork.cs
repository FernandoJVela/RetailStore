using Microsoft.EntityFrameworkCore.Storage;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly RetailStoreDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(RetailStoreDbContext context)
        => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct)
        => _transaction = await _context.Database
            .BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
        await _transaction!.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(ct);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
