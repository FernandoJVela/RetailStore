using RetailStore.Api.Features.Customers.Application;
using RetailStore.Api.Features.Customers.Domain;
using RetailStore.Api.Features.Customers.Infrastructure;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Customers;
 
public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRepository<Customer>>(sp =>
            sp.GetRequiredService<ICustomerRepository>());
        return services;
    }
}