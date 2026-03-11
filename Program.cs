using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Products;
// using RetailStore.Api.Features.Customers;
// using RetailStore.Api.Features.Orders;
// using RetailStore.Api.Features.Inventory;
// using RetailStore.Api.Features.Providers;
// using RetailStore.Api.Features.Users;
using RetailStore.Infrastructure.Behaviors;
using RetailStore.Infrastructure.Outbox;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ──────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// ─── EF Core + SQL Server ─────────────────────────────
builder.Services.AddDbContext<RetailStoreDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration
            .GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(
            typeof(RetailStoreDbContext).Assembly.FullName)));

// ─── Unit of Work ─────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── MediatR + Pipeline ───────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(
        typeof(RetailStoreDbContext).Assembly);
});
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

// ─── FluentValidation ─────────────────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly);

// ─── Feature Modules ──────────────────────────────────
builder.Services
    .AddProductsModule();
    // .AddCustomersModule()
    // .AddOrdersModule()
    // .AddInventoryModule()
    // .AddProvidersModule()
    // .AddUsersModule();

// ─── Outbox Processor ─────────────────────────────────
builder.Services.AddHostedService<OutboxProcessor>();

// ─── API ──────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for EF Core migrations
public partial class Program { }