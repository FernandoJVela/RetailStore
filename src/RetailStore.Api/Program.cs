using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Configuration;
using RetailStore.Api.Middleware;
using RetailStore.Infrastructure.Behaviors;
using RetailStore.Infrastructure.Outbox;
using RetailStore.Infrastructure.Persistence;
using RetailStore.Api.Features.Products;
using RetailStore.Api.Features.Customers;
using RetailStore.Api.Features.Orders;
using RetailStore.Api.Features.Inventory;
// using RetailStore.Api.Features.Providers;
using RetailStore.Api.Features.Users;
using HealthChecks.UI.Client;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Observability (Serilog + OpenTelemetry + Health) ─────
builder.AddObservability();

// ─── EF Core Assembly Configuration ─────────────────────
builder.Services.AddSingleton(new DbContextAssemblyOptions
{
    ConfigurationAssemblies = { typeof(Program).Assembly }
});

// ─── EF Core ──────────────────────────────────────────────
builder.Services.AddDbContext<RetailStoreDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Caching ──────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ─── MediatR + Pipeline (ORDER = EXECUTION ORDER) ────────
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CorrelationBehavior<,>));   // 1
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));       // 2
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));       // 3
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));       // 4
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>)); // 5
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));    // 6
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)); // 7
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));    // 8

// ─── Validation ──────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// ─── Exception Handling + ProblemDetails ──────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Resilience ──────────────────────────────────────────
builder.Services.AddResiliencePolicies();
builder.Services.AddRateLimiting();

// ─── Feature Modules ────────────────────────────────────
builder.Services.AddProductsModule()
    .AddCustomersModule()
    .AddOrdersModule()
    .AddInventoryModule()
    // .AddProvidersModule()
    .AddUsersModule();

// ─── Outbox ──────────────────────────────────────────────
builder.Services.AddHostedService<OutboxProcessor>();

// ─── HTTP ────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "RetailStore API", Version = "v1" });

    const string schemeId = "bearer";

    options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese su token JWT"
    });

    // 2. Security requirement (.NET 10)
    // Use a delegate to reference this definition on a secure way
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeId, document)] = []
    });
});
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// ─── Middleware Pipeline (ORDER MATTERS!) ────────────────
app.UseMiddleware<CorrelationMiddleware>();  // 1st: IDs
app.UseExceptionHandler();                   // 2nd: catch all
app.UseRateLimiter();                        // 3rd: protect
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new()
    { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.Run();

public partial class Program { }