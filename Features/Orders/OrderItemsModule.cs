using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Api.Features.Orders.Infrastructure;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Orders;

public static class OrdersItemsModule
{
    public static IServiceCollection AddOrderItemsModule(
        this IServiceCollection services)
    {
        services.AddScoped<IRepository<OrderItem>,
            OrderItemRepository>();
        return services;
    }
}
