using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using RetailStore.Infrastructure.Persistence;

namespace RetailStore.Infrastructure.Outbox;

public sealed class OutboxHealthCheck : IHealthCheck
{
    private readonly RetailStoreDbContext _db;
    private const int MaxUnprocessedMessages = 50;

    public OutboxHealthCheck(RetailStoreDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken ct = default)
    {
        try
        {
            // Check how many messages are stuck or pending
            var pendingMessages = await _db.OutboxMessages
                .CountAsync(m => m.ProcessedOn == null, ct);

            if (pendingMessages <= MaxUnprocessedMessages)
            {
                return HealthCheckResult.Healthy($"Outbox is healthy ({pendingMessages} pending).");
            }

            return HealthCheckResult.Degraded($"Outbox is lagging: {pendingMessages} messages pending.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Could not connect to Outbox table.", ex);
        }
    }
}