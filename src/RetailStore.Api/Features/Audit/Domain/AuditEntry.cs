namespace RetailStore.Api.Features.Audit.Domain;
 
/// <summary>
/// Immutable audit log entry. Created by AuditBehavior, never modified.
/// Not a DDD aggregate — this is an infrastructure concern stored via DbContext.
/// </summary>
public sealed class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
 
    // Who
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
 
    // What
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Description { get; set; }
 
    // Payload
    public string? RequestPayload { get; set; }
    public string? ResponseSummary { get; set; }
    public string? ChangedProperties { get; set; }
 
    // Result
    public string Outcome { get; set; } = "Success";
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
 
    // Context
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}