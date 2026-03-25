using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RetailStore.Infrastructure.Outbox;
using Serilog;

namespace RetailStore.Api.Configuration;

public static class ObservabilityConfig
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        // ─── Serilog ──────────────────────────────────────────
        builder.Host.UseSerilog((ctx, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "RetailStore")
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

        // ─── OpenTelemetry ────────────────────────────────────
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("RetailStore.Api",
                serviceVersion: "3.0.0"))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("RetailStore.*")
                .AddOtlpExporter())
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("RetailStore.*")
                .AddOtlpExporter());

        // ─── Health Checks ────────────────────────────────────
        builder.Services.AddHealthChecks()
            .AddSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")!,
                name: "sqlserver",
                tags: new[] { "db", "critical" })
            .AddCheck<OutboxHealthCheck>(
                "outbox-depth",
                tags: new[] { "outbox", "critical" });

        return builder;
    }
}