using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Notifications.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Application.Queries;
 
// ─── DTOs ───────────────────────────────────────────────────
public sealed record NotificationDto(
    Guid Id, string Channel, string Category,
    string? Subject, string Status, string Priority,
    DateTime CreatedAt, DateTime? ReadAt);
 
public sealed record NotificationDetailDto(
    Guid Id, Guid? TemplateId, Guid? RecipientId,
    string RecipientType, string Channel, string Category,
    string? Subject, string Body, string Status, string Priority,
    DateTime? SentAt, DateTime? DeliveredAt, DateTime? ReadAt,
    DateTime? FailedAt, string? FailureReason, int RetryCount,
    string? ReferenceType, Guid? ReferenceId,
    DateTime CreatedAt);
 
public sealed record PreferenceDto(
    Guid Id, string Category, string Channel, bool IsEnabled);
 
// ═══════════════════════════════════════════════════════════
// GET NOTIFICATIONS FOR RECIPIENT
// ═══════════════════════════════════════════════════════════
public sealed record GetNotificationsQuery(
    Guid RecipientId, string? Status = null,
    string? Category = null, int Limit = 50
) : IQuery<List<NotificationDto>>;
 
public sealed class GetNotificationsHandler(RetailStoreDbContext db)
    : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery q, CancellationToken ct)
    {
        var query = db.Set<Notification>().AsNoTracking()
            .Where(n => n.RecipientId == q.RecipientId);
 
        if (!string.IsNullOrEmpty(q.Status) && Enum.TryParse<NotificationStatus>(q.Status, true, out var status))
            query = query.Where(n => n.Status == status);
        if (!string.IsNullOrEmpty(q.Category) && Enum.TryParse<NotificationCategory>(q.Category, true, out var cat))
            query = query.Where(n => n.Category == cat);
 
        return await query.OrderByDescending(n => n.CreatedAt).Take(q.Limit)
            .Select(n => new NotificationDto(
                n.Id, n.Channel.ToString(), n.Category.ToString(),
                n.Subject, n.Status.ToString(), n.Priority.ToString(),
                n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET NOTIFICATION BY ID
// ═══════════════════════════════════════════════════════════
public sealed record GetNotificationByIdQuery(Guid Id) : IQuery<NotificationDetailDto>;
 
public sealed class GetNotificationByIdHandler(RetailStoreDbContext db)
    : IRequestHandler<GetNotificationByIdQuery, NotificationDetailDto>
{
    public async Task<NotificationDetailDto> Handle(GetNotificationByIdQuery q, CancellationToken ct)
    {
        var n = await db.Set<Notification>().AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == q.Id, ct)
            ?? throw new DomainException(NotificationErrors.NotFound(q.Id));
 
        return new NotificationDetailDto(
            n.Id, n.TemplateId, n.RecipientId,
            n.RecipientType.ToString(), n.Channel.ToString(), n.Category.ToString(),
            n.Subject, n.Body, n.Status.ToString(), n.Priority.ToString(),
            n.SentAt, n.DeliveredAt, n.ReadAt, n.FailedAt, n.FailureReason, n.RetryCount,
            n.ReferenceType, n.ReferenceId, n.CreatedAt);
    }
}
 
// ═══════════════════════════════════════════════════════════
// GET UNREAD COUNT
// ═══════════════════════════════════════════════════════════
public sealed record GetUnreadCountQuery(Guid RecipientId) : IQuery<int>;
 
public sealed class GetUnreadCountHandler(RetailStoreDbContext db)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    public async Task<int> Handle(GetUnreadCountQuery q, CancellationToken ct)
        => await db.Set<Notification>().AsNoTracking()
            .CountAsync(n => n.RecipientId == q.RecipientId
                && n.Status != NotificationStatus.Read
                && n.Status != NotificationStatus.Cancelled
                && n.Status != NotificationStatus.Failed, ct);
}
 
// ═══════════════════════════════════════════════════════════
// GET PREFERENCES
// ═══════════════════════════════════════════════════════════
public sealed record GetPreferencesQuery(Guid RecipientId) : IQuery<List<PreferenceDto>>;
 
public sealed class GetPreferencesHandler(RetailStoreDbContext db)
    : IRequestHandler<GetPreferencesQuery, List<PreferenceDto>>
{
    public async Task<List<PreferenceDto>> Handle(GetPreferencesQuery q, CancellationToken ct)
        => await db.Set<NotificationPreference>().AsNoTracking()
            .Where(p => p.RecipientId == q.RecipientId)
            .Select(p => new PreferenceDto(
                p.Id, p.Category.ToString(), p.Channel.ToString(), p.IsEnabled))
            .ToListAsync(ct);
}