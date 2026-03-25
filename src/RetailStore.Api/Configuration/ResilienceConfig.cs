using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace RetailStore.Api.Configuration;

public static class ResilienceConfig
{
    public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
    {
        // ─── Retry Policy (for HttpClient) ────────────────────
        services.AddHttpClient("ExternalApi")
            .AddResilienceHandler("retry-pipeline", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(60)
                });

                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            // Per-tenant rate limit (bulkhead isolation)
            opts.AddPolicy("per-tenant", ctx =>
            {
                var tenantId = ctx.User?.FindFirst("tenant_id")?.Value ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(tenantId,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            // Global rate limit
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                ctx => RateLimitPartition.GetFixedWindowLimiter("global",
                    _ => new FixedWindowRateLimiterOptions
                    { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) }));

            opts.RejectionStatusCode = 429;
        });

        return services;
    }
}
