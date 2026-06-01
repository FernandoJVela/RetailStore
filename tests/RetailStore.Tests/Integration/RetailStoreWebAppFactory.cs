using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailStore.Infrastructure.Outbox;
using RetailStore.Infrastructure.Persistence;
using RetailStore.Tests.TestHelpers;

namespace RetailStore.Tests.Integration;

/// <summary>
/// Shared WebApplicationFactory for integration tests.
/// Replaces SQL Server with a SQLite shared-memory database so tests run
/// without any external infrastructure.
///
/// Usage: implement IClassFixture&lt;RetailStoreWebAppFactory&gt; on your test class.
/// Each test class gets its own factory (and thus its own isolated database).
/// </summary>
public class RetailStoreWebAppFactory : WebApplicationFactory<Program>
{
    // Unique DB name per factory instance → full test-class isolation
    private readonly string _dbName = $"Test_{Guid.NewGuid():N}";

    // A persistent connection that keeps the shared in-memory DB alive for the factory's lifetime.
    // SQLite in-memory databases are destroyed when the last connection closes.
    private SqliteConnection? _keepAlive;

    // JWT config used both for the app (via ConfigureAppConfiguration) and for token generation
    public const string JwtSecret   = "TestSecret_MustBeAtLeast32Chars_ForHmacSha256!!";
    public const string JwtIssuer   = "RetailStoreTest";
    public const string JwtAudience = "RetailStoreTest";

    public RetailStoreWebAppFactory()
    {
        // Open before the host builds so DatabaseSeeder.SeedAsync() (called during startup)
        // can use EnsureCreated() against an already-live SQLite database.
        _keepAlive = new SqliteConnection(ConnectionString);
        _keepAlive.Open();
    }

    private string ConnectionString => $"DataSource={_dbName};Mode=Memory;Cache=Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ── Override app configuration ──────────────────────────────────────
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]        = JwtSecret,
                ["Jwt:Issuer"]        = JwtIssuer,
                ["Jwt:Audience"]      = JwtAudience,
                ["Jwt:ExpiryMinutes"] = "60",
                // Leave Seed:AdminEmail unset so DatabaseSeeder skips user assignment
            });
        });

        // ── Override services ───────────────────────────────────────────────
        builder.ConfigureServices(services =>
        {
            // EF Core registers one IDbContextOptionsConfiguration<T> per AddDbContext call.
            // Each holds the lambda that called UseSqlServer / UseSqlite. If both are present
            // EF Core rejects startup with "multiple providers registered". Remove the SQL Server
            // configuration before adding SQLite so only one provider is ever registered.
            var optionsConfigType = typeof(IDbContextOptionsConfiguration<RetailStoreDbContext>);
            foreach (var d in services.Where(d => d.ServiceType == optionsConfigType).ToList())
                services.Remove(d);

            // Also remove the cached DbContextOptions if present
            RemoveDescriptor<DbContextOptions<RetailStoreDbContext>>(services);

            // Register fresh with SQLite shared in-memory
            services.AddDbContext<RetailStoreDbContext>((sp, opts) =>
            {
                opts.UseSqlite(ConnectionString);
            });

            // Remove the OutboxProcessor background service — it polls the DB on a timer
            // and is not needed (nor desirable) during test runs
            RemoveDescriptor<OutboxProcessor>(services, byImplementationType: true);
        });
    }

    // ── Client factory helpers ──────────────────────────────────────────────

    /// <summary>Creates an HttpClient authenticated as admin (permission "*:*").</summary>
    public HttpClient CreateAdminClient()
        => CreateClientWithPermissions("*:*");

    /// <summary>Creates an HttpClient authenticated with the given permissions.</summary>
    public HttpClient CreateClientWithPermissions(params string[] permissions)
    {
        var client = CreateClient();
        var token  = TestJwtHelper.GenerateToken(permissions, JwtSecret, JwtIssuer, JwtAudience);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── DbContext helper ────────────────────────────────────────────────────

    /// <summary>
    /// Runs an action inside a fresh DI scope with its own DbContext.
    /// Used by tests for assertion-side reads or test-setup seeding.
    /// SaveChanges() (sync) is used deliberately to bypass the domain-event
    /// outbox override in RetailStoreDbContext.SaveChangesAsync().
    /// </summary>
    public void UseDbContext(Action<RetailStoreDbContext> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailStoreDbContext>();
        action(db);
    }

    public T UseDbContext<T>(Func<RetailStoreDbContext, T> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailStoreDbContext>();
        return action(db);
    }

    // ── Cleanup ─────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAlive?.Dispose();
            _keepAlive = null;
        }
        base.Dispose(disposing);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private static void RemoveDescriptor<T>(
        IServiceCollection services, bool byImplementationType = false)
    {
        var descriptor = byImplementationType
            ? services.SingleOrDefault(d => d.ImplementationType == typeof(T))
            : services.SingleOrDefault(d => d.ServiceType == typeof(T));

        if (descriptor != null)
            services.Remove(descriptor);
    }
}
