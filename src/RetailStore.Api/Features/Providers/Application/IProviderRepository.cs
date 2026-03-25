using RetailStore.Api.Features.Providers.Domain;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Providers.Application;
 
public interface IProviderRepository : IRepository<Provider>
{
    Task<Provider?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
    Task<List<Provider>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
}