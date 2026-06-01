using RetailStore.Infrastructure.Persistence;

namespace RetailStore.Tests.Integration;

/// <summary>
/// Abstract base for integration tests that need a running ASP.NET Core app and a real database.
///
/// Inheriting test classes also need to implement IClassFixture&lt;RetailStoreWebAppFactory&gt;
/// so xUnit injects the factory:
///
///   public class MyTests : IntegrationTestBase, IClassFixture&lt;RetailStoreWebAppFactory&gt;
///   {
///       public MyTests(RetailStoreWebAppFactory factory) : base(factory) { }
///   }
///
/// Database isolation: each test class gets its own factory instance (and thus its own
/// in-memory SQLite database). Tests within a class share the database and run sequentially.
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly RetailStoreWebAppFactory Factory;

    protected IntegrationTestBase(RetailStoreWebAppFactory factory)
    {
        Factory = factory;
    }

    // ── HTTP clients ────────────────────────────────────────────────────────

    /// <summary>Unauthenticated client — use for 401 / public endpoint tests.</summary>
    protected HttpClient CreateAnonymousClient() => Factory.CreateClient();

    /// <summary>Admin client with *:* permission — passes every authorization check.</summary>
    protected HttpClient CreateAdminClient() => Factory.CreateAdminClient();

    /// <summary>Client authenticated with only the specified permissions.</summary>
    protected HttpClient CreateClientWithPermissions(params string[] permissions)
        => Factory.CreateClientWithPermissions(permissions);

    // ── Database helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Reads from the database in a short-lived scope.
    /// Use for asserting side-effects after an HTTP call.
    /// </summary>
    protected T QueryDb<T>(Func<RetailStoreDbContext, T> query)
        => Factory.UseDbContext(query);

    /// <summary>
    /// Seeds entities directly into the database before a test.
    /// Uses SaveChanges() (sync) to bypass the domain-event outbox.
    /// </summary>
    protected void Seed(params object[] entities)
        => Factory.UseDbContext(db =>
        {
            foreach (var entity in entities) db.Add(entity);
            db.SaveChanges();
        });
}
