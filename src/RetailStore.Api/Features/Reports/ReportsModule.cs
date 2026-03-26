namespace RetailStore.Api.Features.Reports;
 
public static class ReportsModule
{
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        // Reports module is query-only. No repositories needed.
        // All queries go through DbContext directly (CQRS read side).
        return services;
    }
}