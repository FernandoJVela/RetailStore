using RetailStore.Api.Features.Products.Domain;
using RetailStore.Api.Features.Products.Infrastructure;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Products;

public static class ProductsModule
{
    public static IServiceCollection AddProductsModule(
        this IServiceCollection services)
    {
        services.AddScoped<IRepository<Product>,
            ProductRepository>();
        return services;
    }
}
