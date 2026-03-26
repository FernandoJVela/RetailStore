using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Audit.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
 
namespace RetailStore.Api.Features.Audit.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record AuditEntryDto(
    Guid Id, DateTime Timestamp, string? Username,
    string Action, string Module,
    string? EntityType, string? EntityId,
    string? Description, string Outcome,
    int DurationMs, string? CorrelationId);
 
public sealed record AuditDetailDto(
    Guid Id, DateTime Timestamp,
    Guid? UserId, string? Username, string? IpAddress,
    string Action, string Module,
    string? EntityType, string? EntityId, string? Description,
    string? RequestPayload, string? ResponseSummary,
    string? ChangedProperties,
    string Outcome, string? ErrorCode, string? ErrorMessage,
    int DurationMs, string? CorrelationId, string? RequestId);
 
public sealed record ModuleActivityDto(
    string Module, int TotalActions,
    int SuccessCount, int FailureCount,
    int AvgDurationMs, int MaxDurationMs,
    int UniqueUsers, DateTime? LastActivity);
 
public sealed record UserActivityDto(
    Guid? UserId, string? Username,
    int TotalActions, int FailedActions,
    int ModulesAccessed,
    DateTime? FirstAction, DateTime? LastAction);
 
// ═══════════════════════════════════════════════════════════
// SEARCH AUDIT LOG (with filters)
// ═══════════════════════════════════════════════════════════
public sealed record SearchAuditLogQuery(
    Guid? UserId = null,
    string? Module = null,
    string? EntityType = null,
    string? EntityId = null,
    string? Outcome = null,
    DateTime? From = null,
    DateTime? To = null,
    int Limit = 50
) : IQuery<List<AuditEntryDto>>;
 
public sealed class SearchAuditLogHandler(RetailStoreDbContext db)
    : IRequestHandler<SearchAuditLogQuery, List<AuditEntryDto>>
{
    public async Task<List<AuditEntryDto>> Handle(SearchAuditLogQuery q, CancellationToken ct)
    {
        var query = db.Set<AuditEntry>().AsNoTracking().AsQueryable();
 
        if (q.UserId.HasValue)
            query = query.Where(a => a.UserId == q.UserId.Value);
        if (!string.IsNullOrEmpty(q.Module))
            query = query.Where(a => a.Module == q.Module);
        if (!string.IsNullOrEmpty(q.EntityType))
            query = query.Where(a => a.EntityType == q.EntityType);
        if (!string.IsNullOrEmpty(q.EntityId))
            query = query.Where(a => a.EntityId == q.EntityId);
        if (!string.IsNullOrEmpty(q.Outcome))
            query = query.Where(a => a.Outcome == q.Outcome);
        if (q.From.HasValue)
            query = query.Where(a => a.Timestamp >= q.From.Value);
        if (q.To.HasValue)
            query = query.Where(a => a.Timestamp <= q.To.Value);
 
        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(q.Limit)
            .Select(a => new AuditEntryDto(
                a.Id, a.Timestamp, a.Username,
                a.Action, a.Module, a.EntityType, a.EntityId,
                a.Description, a.Outcome, a.DurationMs, a.CorrelationId))
            .ToListAsync(ct);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET AUDIT ENTRY DETAIL
// ═══════════════════════════════════════════════════════════
public sealed record GetAuditEntryByIdQuery(Guid Id) : IQuery<AuditDetailDto>;
 
public sealed class GetAuditEntryByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetAuditEntryByIdQuery, AuditDetailDto>
{
    public async Task<AuditDetailDto> Handle(GetAuditEntryByIdQuery q, CancellationToken ct)
    {
        var a = await db.Set<AuditEntry>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == q.Id, ct)
            ?? throw new SharedKernel.Domain.DomainException(
                new SharedKernel.Domain.DomainError("AUDIT_NOT_FOUND",
                    $"Audit entry '{q.Id}' not found.",
                    SharedKernel.Domain.DomainErrorType.NotFound));
 
        return new AuditDetailDto(
            a.Id, a.Timestamp, a.UserId, a.Username, a.IpAddress,
            a.Action, a.Module, a.EntityType, a.EntityId, a.Description,
            a.RequestPayload, a.ResponseSummary, a.ChangedProperties,
            a.Outcome, a.ErrorCode, a.ErrorMessage,
            a.DurationMs, a.CorrelationId, a.RequestId);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET ENTITY HISTORY (all changes to a specific entity)
// ═══════════════════════════════════════════════════════════
public sealed record GetEntityHistoryQuery(
    string EntityType, string EntityId
) : IQuery<List<AuditEntryDto>>;
 
public sealed class GetEntityHistoryHandler(RetailStoreDbContext db)
    : IRequestHandler<GetEntityHistoryQuery, List<AuditEntryDto>>
{
    public async Task<List<AuditEntryDto>> Handle(GetEntityHistoryQuery q, CancellationToken ct)
    {
        return await db.Set<AuditEntry>().AsNoTracking()
            .Where(a => a.EntityType == q.EntityType && a.EntityId == q.EntityId)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditEntryDto(
                a.Id, a.Timestamp, a.Username,
                a.Action, a.Module, a.EntityType, a.EntityId,
                a.Description, a.Outcome, a.DurationMs, a.CorrelationId))
            .ToListAsync(ct);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET RECENT FAILURES
// ═══════════════════════════════════════════════════════════
public sealed record GetRecentFailuresQuery(int Limit = 50) : IQuery<List<AuditEntryDto>>;
 
public sealed class GetRecentFailuresHandler(RetailStoreDbContext db)
    : IRequestHandler<GetRecentFailuresQuery, List<AuditEntryDto>>
{
    public async Task<List<AuditEntryDto>> Handle(GetRecentFailuresQuery q, CancellationToken ct)
    {
        return await db.Set<AuditEntry>().AsNoTracking()
            .Where(a => a.Outcome != "Success")
            .OrderByDescending(a => a.Timestamp)
            .Take(q.Limit)
            .Select(a => new AuditEntryDto(
                a.Id, a.Timestamp, a.Username,
                a.Action, a.Module, a.EntityType, a.EntityId,
                a.Description, a.Outcome, a.DurationMs, a.CorrelationId))
            .ToListAsync(ct);
    }
}
 
// ═══════════════════════════════════════════════════════════
// ACTIVITY BY MODULE (stats)
// ═══════════════════════════════════════════════════════════
public sealed record GetModuleActivityQuery() : IQuery<List<ModuleActivityDto>>;
 
public sealed class GetModuleActivityHandler(RetailStoreDbContext db)
    : IRequestHandler<GetModuleActivityQuery, List<ModuleActivityDto>>
{
    public async Task<List<ModuleActivityDto>> Handle(GetModuleActivityQuery q, CancellationToken ct)
    {
        var data = await db.Set<AuditEntry>().AsNoTracking().ToListAsync(ct);
 
        return data.GroupBy(a => a.Module)
            .Select(g => new ModuleActivityDto(
                g.Key,
                g.Count(),
                g.Count(a => a.Outcome == "Success"),
                g.Count(a => a.Outcome != "Success"),
                (int)g.Average(a => a.DurationMs),
                g.Max(a => a.DurationMs),
                g.Where(a => a.UserId.HasValue).Select(a => a.UserId).Distinct().Count(),
                g.Max(a => a.Timestamp)))
            .OrderByDescending(m => m.TotalActions)
            .ToList();
    }
}
 
// ═══════════════════════════════════════════════════════════
// USER ACTIVITY SUMMARY
// ═══════════════════════════════════════════════════════════
public sealed record GetUserActivityQuery() : IQuery<List<UserActivityDto>>;
 
public sealed class GetUserActivityHandler(RetailStoreDbContext db)
    : IRequestHandler<GetUserActivityQuery, List<UserActivityDto>>
{
    public async Task<List<UserActivityDto>> Handle(GetUserActivityQuery q, CancellationToken ct)
    {
        var data = await db.Set<AuditEntry>().AsNoTracking()
            .Where(a => a.UserId != null)
            .ToListAsync(ct);
 
        return data.GroupBy(a => new { a.UserId, a.Username })
            .Select(g => new UserActivityDto(
                g.Key.UserId,
                g.Key.Username,
                g.Count(),
                g.Count(a => a.Outcome != "Success"),
                g.Select(a => a.Module).Distinct().Count(),
                g.Min(a => a.Timestamp),
                g.Max(a => a.Timestamp)))
            .OrderByDescending(u => u.TotalActions)
            .ToList();
    }
}