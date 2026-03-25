using RetailStore.Api.Features.Providers.Application;
using RetailStore.Api.Features.Providers.Domain;
using RetailStore.Api.Features.Providers.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Providers;
 
public static class ProvidersModule
{
    public static IServiceCollection AddProvidersModule(this IServiceCollection services)
    {
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IRepository<Provider>>(sp =>
            sp.GetRequiredService<IProviderRepository>());
        return services;
    }
}