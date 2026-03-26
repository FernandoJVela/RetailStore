using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Payments.Application;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Payments.Infrastructure;
 
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly RetailStoreDbContext _db;
    public PaymentRepository(RetailStoreDbContext db) => _db = db;
 
    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Payment>()
            .Include(p => p.Refunds)
            .ToListAsync(ct);
 
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Payment>()
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
 
    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await _db.Set<Payment>()
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
 
    public async Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken ct = default)
        => await _db.Set<Payment>().AnyAsync(p => p.OrderId == orderId, ct);
 
    public async Task AddAsync(Payment entity, CancellationToken ct = default)
        => await _db.Set<Payment>().AddAsync(entity, ct);
 
    public void Update(Payment entity) => _db.Set<Payment>().Update(entity);
    public void Remove(Payment entity) => _db.Set<Payment>().Remove(entity);
}