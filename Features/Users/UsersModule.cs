using RetailStore.Api.Features.Users.Domain;
using RetailStore.Api.Features.Users.Infrastructure;
using RetailStore.SharedKernel.Application;

namespace RetailStore.Api.Features.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IRepository<User>, UserRepository>();
        services.AddScoped<IRepository<Role>, RoleRepository>();
        services.AddScoped<IPermissionService, CachedPermissionService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}