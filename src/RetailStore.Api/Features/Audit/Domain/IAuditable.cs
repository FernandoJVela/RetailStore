namespace RetailStore.Api.Features.Audit.Domain;
 
/// <summary>
/// Marker interface for commands that should generate an audit trail entry.
/// Apply to any ICommand that mutates state.
/// 
/// The AuditBehavior pipeline behavior detects this interface and automatically
/// creates an AuditEntry with the command details, user context, timing,
/// and outcome (success or failure).
/// 
/// Usage:
///   public sealed record CreateProductCommand(...) : ICommand&lt;Guid&gt;, IAuditable
///   {
///       public string AuditModule => "Products";
///       public string AuditDescription => $"Creating product {Name} ({Sku})";
///   }
/// 
/// If your command doesn't implement IAuditable, it won't be audited.
/// Queries are never audited (read operations don't mutate state).
/// </summary>
public interface IAuditable
{
    /// <summary>Module name for grouping (e.g., "Products", "Orders", "Users")</summary>
    string AuditModule { get; }
 
    /// <summary>Optional human-readable description. Can reference command properties.</summary>
    string? AuditDescription => null;
}