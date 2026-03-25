using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Orders.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Orders;
 
public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddScoped<IRepository<Order>, OrderRepository>();
        return services;
    }
}