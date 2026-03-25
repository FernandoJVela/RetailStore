using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Shipping.Application;
 
public interface IShipmentRepository : IRepository<Shipment>
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default);
    Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken ct = default);
}