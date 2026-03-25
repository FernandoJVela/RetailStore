using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private const int BatchSize = 20;
    private const int PollingIntervalMs = 2000;
    private const int MaxRetries = 3;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing outbox messages");
            }
            await Task.Delay(PollingIntervalMs, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages(
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<RetailStoreDbContext>();
        var publisher = scope.ServiceProvider
            .GetRequiredService<IPublisher>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOn == null
                     && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var domainEvent = JsonConvert
                    .DeserializeObject<IDomainEvent>(
                    message.Content,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling =
                            TypeNameHandling.All
                    })!;

                await publisher.Publish(domainEvent, ct);
                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                _logger.LogError(ex,
                    "Failed to process outbox {Id}",
                    message.Id);
            }
        }
        await db.SaveChangesAsync(ct);
    }
}