using RetailStore.SharedKernel.Application;
using RetailStore.Api.Features.Customers.Domain;
 
namespace RetailStore.Api.Features.Customers.Application;
 
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
}