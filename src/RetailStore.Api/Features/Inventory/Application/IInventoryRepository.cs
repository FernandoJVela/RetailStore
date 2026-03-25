using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Inventory.Application;
 
public interface IInventoryRepository : IRepository<InventoryItem>
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct = default);
}