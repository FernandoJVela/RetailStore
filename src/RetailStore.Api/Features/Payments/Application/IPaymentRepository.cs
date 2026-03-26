using RetailStore.Api.Features.Payments.Domain;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Payments.Application;
 
public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken ct = default);
}