using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Orders.Domain;
using RetailStore.Infrastructure.Persistence;

namespace RetailStore.Tests.TestHelpers;

public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates a SQLite in-memory DbContext per test. SQLite is used instead of the
    /// EF Core InMemory provider because ComplexProperty (used for Money) has a known
    /// bug in the InMemory provider when HasColumnName is applied.
    ///
    /// Each call opens a dedicated SqliteConnection that the caller must dispose along
    /// with the DbContext — use the overload that returns the connection, or rely on
    /// the DbContext's Dispose to close it via the owned-connection pattern.
    ///
    /// Entity configurations from RetailStore.Api (Feature/.../Infrastructure) are
    /// registered automatically via DbContextAssemblyOptions.
    /// </summary>
    public static RetailStoreDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<RetailStoreDbContext>()
            .UseSqlite(connection)
            .Options;

        var asmOptions = new DbContextAssemblyOptions();
        // Entity configurations (IEntityTypeConfiguration<T>) live in the Api assembly
        asmOptions.ConfigurationAssemblies.Add(typeof(Order).Assembly);

        var db = new RetailStoreDbContext(options, asmOptions);
        db.Database.EnsureCreated();
        return db;
    }
}
