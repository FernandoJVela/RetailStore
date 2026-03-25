using RetailStore.Api.Features.Inventory.Application;
using RetailStore.Api.Features.Inventory.Domain;
using RetailStore.Api.Features.Inventory.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Inventory;
 
public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IRepository<InventoryItem>>(sp =>
            sp.GetRequiredService<IInventoryRepository>());
        return services;
    }
}