using MediatR;
using RetailStore.Api.Features.Audit.Infrastructure;
 
namespace RetailStore.Api.Features.Audit;
 
public static class AuditModule
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
 
        return services;
    }
}