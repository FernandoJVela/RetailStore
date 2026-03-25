using RetailStore.Api.Features.Shipping.Application;
using RetailStore.Api.Features.Shipping.Domain;
using RetailStore.Api.Features.Shipping.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Shipping;
 
public static class ShippingModule
{
    public static IServiceCollection AddShippingModule(this IServiceCollection services)
    {
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IRepository<Shipment>>(sp =>
            sp.GetRequiredService<IShipmentRepository>());
        return services;
    }
}