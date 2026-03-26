namespace RetailStore.Api.Features.Notifications;
 
public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        // Notifications module uses DbContext directly (no repository needed)
        // All entities are managed through commands/queries against the DbContext
        return services;
    }
}