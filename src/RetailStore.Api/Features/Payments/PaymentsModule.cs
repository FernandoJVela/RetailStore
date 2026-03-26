using RetailStore.Api.Features.Payments.Application;
using RetailStore.Api.Features.Payments.Domain;
using RetailStore.Api.Features.Payments.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Payments;
 
public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRepository<Payment>>(sp =>
            sp.GetRequiredService<IPaymentRepository>());
        return services;
    }
}