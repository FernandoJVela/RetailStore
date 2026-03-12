using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RetailStore.Api.Features.Products;
// using RetailStore.Api.Features.Customers;
// using RetailStore.Api.Features.Orders;
// using RetailStore.Api.Features.Inventory;
// using RetailStore.Api.Features.Providers;
// using RetailStore.Api.Features.Users;
using RetailStore.Api.Middleware;
using RetailStore.Infrastructure.Behaviors;
using RetailStore.Infrastructure.Outbox;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ──────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()            // CorrelationId enrichment
    .Enrich.WithProperty("Application", "RetailStore")
    .WriteTo.Console());

// ─── EF Core ──────────────────────────────────────────────
builder.Services.AddDbContext<RetailStoreDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// ─── MediatR + Pipeline (ORDER MATTERS!) ─────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Pipeline registration order = execution order
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

// ─── FluentValidation ─────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// ─── Global Exception Handler ─────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Feature Modules ──────────────────────────────────────
builder.Services
    .AddProductsModule();
    // .AddCustomersModule()
    // .AddOrdersModule()
    // .AddInventoryModule()
    // .AddProvidersModule()
    // .AddUsersModule();

// ─── Outbox ────────────────────────────────────────────────
builder.Services.AddHostedService<OutboxProcessor>();

// ─── OpenTelemetry ────────────────────────────────────────
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("RetailStore.Api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("RetailStore.*")
        .AddOtlpExporter());

// ─── HTTP ─────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

// ─── Middleware Pipeline (ORDER MATTERS!) ─────────────────
app.UseMiddleware<CorrelationIdMiddleware>();  // First: set ID
app.UseExceptionHandler();                     // Second: catch all
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }