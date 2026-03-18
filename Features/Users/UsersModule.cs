using RetailStore.Api.Features.Users.Application;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Api.Features.Users.Infrastructure;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepository<User>>(sp =>
            sp.GetRequiredService<IUserRepository>());
        services.AddScoped<IRepository<Role>, RoleRepository>();  // Add this
        services.AddScoped<IPermissionService, CachedPermissionService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}