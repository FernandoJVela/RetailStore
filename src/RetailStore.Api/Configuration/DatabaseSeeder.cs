using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Configuration;

public static class DatabaseSeeder
{
    // All permissions used across every IRequirePermission command in the codebase.
    private static readonly string[] AdminPermissions = ["*:*"];

    private static readonly string[] StaffPermissions =
    [
        "products:write",
        "customers:write",
        "orders:write",
        "inventory:write",
        "inventory:adjust",
        "providers:write",
        "payments:write",
        "payments:refund",
        "shipping:write",
        "notifications:write",
    ];

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailStoreDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.EnsureCreatedAsync();

        await SeedRolesAsync(db, logger);
        await AssignAdminRoleAsync(db, config, logger);
    }

    // ─── Roles ────────────────────────────────────────────────

    private static async Task SeedRolesAsync(RetailStoreDbContext db, ILogger logger)
    {
        var adminExists = await db.Set<Role>().AnyAsync(r => r.Name == "Admin");
        if (!adminExists)
        {
            var admin = Role.Create("Admin", "Full system access — unrestricted.", isSystem: true);
            foreach (var p in AdminPermissions)
                admin.AddPermission(Permission.Parse(p));

            db.Set<Role>().Add(admin);
            logger.LogInformation("Seeded role: Admin (*:*)");
        }

        var staffExists = await db.Set<Role>().AnyAsync(r => r.Name == "Staff");
        if (!staffExists)
        {
            var staff = Role.Create("Staff", "Standard write access across all modules.", isSystem: false);
            foreach (var p in StaffPermissions)
                staff.AddPermission(Permission.Parse(p));

            db.Set<Role>().Add(staff);
            logger.LogInformation("Seeded role: Staff ({Count} permissions)", StaffPermissions.Length);
        }

        await db.SaveChangesAsync();
    }

    // ─── Admin user assignment ─────────────────────────────────

    private static async Task AssignAdminRoleAsync(
        RetailStoreDbContext db, IConfiguration config, ILogger logger)
    {
        var adminEmail = config["Seed:AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            logger.LogWarning(
                "Seed:AdminEmail is not configured — skipping admin role assignment. " +
                "Set it in appsettings to auto-assign the Admin role on first run.");
            return;
        }

        var adminRole = await db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null) return;

        // Load users in memory: Email is a value-object column,
        // string comparison happens after EF materialisation.
        var users = await db.Set<User>().ToListAsync();
        var user = users.FirstOrDefault(
            u => u.Email.Value.Equals(adminEmail.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            logger.LogWarning(
                "Seed:AdminEmail '{Email}' not found — register first, then restart to auto-assign Admin role.",
                adminEmail);
            return;
        }

        if (user.RoleIds.Contains(adminRole.Id))
        {
            logger.LogDebug("User '{Email}' already has the Admin role.", adminEmail);
            return;
        }

        user.AssignRole(adminRole.Id, adminRole.Name);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Admin role assigned to '{Email}'.", adminEmail);
    }
}
