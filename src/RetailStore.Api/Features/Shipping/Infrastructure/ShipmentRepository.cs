using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Shipping.Application;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Infrastructure.Persistence;
 
namespace RetailStore.Api.Features.Shipping.Infrastructure;
 
public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly RetailStoreDbContext _db;
    public ShipmentRepository(RetailStoreDbContext db) => _db = db;
 
    public async Task<IReadOnlyList<Shipment>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<Shipment>()
            .Include(s => s.Items)
            .ToListAsync(ct);
 
    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Shipment>()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
 
    public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await _db.Set<Shipment>()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, ct);
 
    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
        => await _db.Set<Shipment>()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);
 
    public async Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken ct = default)
        => await _db.Set<Shipment>().AnyAsync(s => s.OrderId == orderId, ct);
 
    public async Task AddAsync(Shipment entity, CancellationToken ct = default)
        => await _db.Set<Shipment>().AddAsync(entity, ct);
 
    public void Update(Shipment entity)
        => _db.Set<Shipment>().Update(entity);
 
    public void Remove(Shipment entity)
        => _db.Set<Shipment>().Remove(entity);
}