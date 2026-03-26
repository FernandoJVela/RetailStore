using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Audit.Infrastructure;
 
/// <summary>
/// MediatR pipeline behavior that automatically creates audit log entries
/// for any command implementing IAuditable.
///
/// Pipeline position: should run AFTER AuthorizationBehavior but BEFORE UnitOfWorkBehavior
/// so the audit entry captures the command result and any domain exceptions.
///
/// Register in Program.cs:
///   builder.Services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(AuditBehavior&lt;,&gt;));
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse>(
    RetailStoreDbContext db,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
 
    // Properties to redact from the request payload
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "PasswordSalt", "Secret",
        "Token", "AccessToken", "RefreshToken",
        "CreditCardNumber", "Cvv", "CardNumber"
    };
 
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Only audit commands that implement IAuditable
        if (request is not IAuditable auditable)
            return await next(ct);
 
        var sw = Stopwatch.StartNew();
        var entry = new AuditEntry
        {
            Action = typeof(TRequest).Name,
            Module = auditable.AuditModule,
            Description = auditable.AuditDescription,
            RequestPayload = SerializeRequest(request),
            Timestamp = DateTime.UtcNow
        };
 
        // Extract user context from HTTP request
        ExtractUserContext(entry);
 
        try
        {
            var response = await next(ct);
 
            sw.Stop();
            entry.DurationMs = (int)sw.ElapsedMilliseconds;
            entry.Outcome = "Success";
 
            // Try to extract the entity ID from the response (if it's a Guid)
            if (response is Guid guidResult)
            {
                entry.EntityId = guidResult.ToString();
                entry.ResponseSummary = $"Created: {guidResult}";
            }
            else
            {
                entry.ResponseSummary = "Completed";
            }
 
            // Extract entity type from command name (e.g., "CreateProductCommand" → "Product")
            entry.EntityType = ExtractEntityType(typeof(TRequest).Name);
 
            await SaveAuditEntry(entry, ct);
            return response;
        }
        catch (DomainException ex)
        {
            sw.Stop();
            entry.DurationMs = (int)sw.ElapsedMilliseconds;
            entry.Outcome = "Failure";
            entry.ErrorCode = ex.Error.Code;
            entry.ErrorMessage = ex.Error.Message;
            entry.ResponseSummary = $"Failed: {ex.Error.Code}";
            entry.EntityType = ExtractEntityType(typeof(TRequest).Name);
 
            await SaveAuditEntry(entry, ct);
            throw; // Re-throw so the ExceptionHandlingBehavior handles it
        }
        catch (Exception ex)
        {
            sw.Stop();
            entry.DurationMs = (int)sw.ElapsedMilliseconds;
            entry.Outcome = "Error";
            entry.ErrorCode = "INTERNAL_ERROR";
            entry.ErrorMessage = ex.Message.Length > 500
                ? ex.Message[..500]
                : ex.Message;
            entry.ResponseSummary = "Internal error";
            entry.EntityType = ExtractEntityType(typeof(TRequest).Name);
 
            await SaveAuditEntry(entry, ct);
            throw;
        }
    }
 
    private void ExtractUserContext(AuditEntry entry)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;
 
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
                entry.UserId = userId;
 
            entry.Username = user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("username")?.Value
                ?? user.Identity.Name;
        }
 
        entry.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
 
        // Correlation and Request IDs from headers
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var corrId))
            entry.CorrelationId = corrId.ToString();
        if (httpContext.Request.Headers.TryGetValue("X-Request-Id", out var reqId))
            entry.RequestId = reqId.ToString();
    }
 
    private static string? SerializeRequest(TRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
 
            // Redact sensitive fields
            var doc = JsonDocument.Parse(json);
            var sanitized = new Dictionary<string, object?>();
 
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (SensitiveProperties.Contains(prop.Name))
                    sanitized[prop.Name] = "***REDACTED***";
                else
                    sanitized[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => prop.Value.GetRawText()
                    };
            }
 
            var result = JsonSerializer.Serialize(sanitized, JsonOptions);
            return result.Length > 4000 ? result[..4000] + "...}" : result;
        }
        catch
        {
            return null;
        }
    }
 
    private static string? ExtractEntityType(string commandName)
    {
        // "CreateProductCommand" → "Product"
        // "UpdateProductPriceCommand" → "Product"
        // "ConfirmOrderCommand" → "Order"
        var name = commandName.Replace("Command", "");
 
        var prefixes = new[] { "Create", "Update", "Delete", "Deactivate", "Reactivate",
            "Register", "Assign", "Revoke", "Add", "Remove", "Mark", "Cancel",
            "Confirm", "Complete", "Fail", "Adjust", "Reserve", "Release", "Fulfill",
            "Change", "Send", "Request", "Set", "Dissociate", "Associate" };
 
        foreach (var prefix in prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var entity = name[prefix.Length..];
                if (!string.IsNullOrEmpty(entity))
                {
                    // "ProductPrice" → "Product", "OrderItem" → "Order"
                    var words = new[] { "Price", "Details", "Stock", "Reservation",
                        "Item", "Email", "Address", "Threshold", "Cost", "Carrier" };
                    foreach (var w in words)
                        if (entity.EndsWith(w)) entity = entity[..^w.Length];
 
                    return string.IsNullOrEmpty(entity) ? null : entity;
                }
            }
        }
 
        return name.Length > 0 ? name : null;
    }
 
    private async Task SaveAuditEntry(AuditEntry entry, CancellationToken ct)
    {
        try
        {
            // Use a separate DbContext scope to avoid interfering with the
            // command's own unit of work / transaction
            await db.Set<AuditEntry>().AddAsync(entry, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Audit failures should NEVER break the main operation
            logger.LogError(ex, "Failed to save audit entry for {Action}", entry.Action);
        }
    }
}