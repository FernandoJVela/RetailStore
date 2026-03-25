using Microsoft.EntityFrameworkCore;
using RetailStore.SharedKernel.Domain;
using Newtonsoft.Json;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Infrastructure.Persistence;

public class RetailStoreDbContext : DbContext
{
    private readonly DbContextAssemblyOptions _assemblyOptions;
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public RetailStoreDbContext(
        DbContextOptions<RetailStoreDbContext> options,
        DbContextAssemblyOptions assemblyOptions)
        : base(options)
    {
        _assemblyOptions = assemblyOptions;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Prevent EF Core from discovering Money as a standalone entity
        modelBuilder.Ignore<Money>();
        
        // Apply all configurations from all feature modules
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RetailStoreDbContext).Assembly);

        // Scan any additional assemblies passed from Program.cs
        foreach (var assembly in _assemblyOptions.ConfigurationAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages", "shared");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ProcessedOn)
             .HasFilter("ProcessedOn IS NULL");
        });
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // Convert domain events to outbox messages before saving
        var aggregateRoots = ChangeTracker.Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents).ToList();

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    }),
                OccurredOn = domainEvent.OccurredOn
            };
            OutboxMessages.Add(outboxMessage);
        }

        aggregateRoots.ForEach(a => a.ClearDomainEvents());

        return await base.SaveChangesAsync(cancellationToken);
    }
}
