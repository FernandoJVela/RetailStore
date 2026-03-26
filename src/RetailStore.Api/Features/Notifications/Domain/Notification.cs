using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Domain;
 
public sealed class Notification : AggregateRoot
{
    public Guid? TemplateId { get; private set; }
    public Guid? RecipientId { get; private set; }
    public RecipientType RecipientType { get; private set; }
    public string? RecipientEmail { get; private set; }
    public string? RecipientPhone { get; private set; }
 
    public NotificationChannel Channel { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
 
    public NotificationStatus Status { get; private set; }
    public NotificationPriority Priority { get; private set; }
 
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
 
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
 
    private Notification() { } // EF Core
 
    // ─── Factory: From template ─────────────────────────────
    public static Notification CreateFromTemplate(
        NotificationTemplate template,
        Dictionary<string, string> variables,
        Guid? recipientId,
        RecipientType recipientType,
        string? recipientEmail = null,
        string? recipientPhone = null,
        NotificationPriority priority = NotificationPriority.Normal,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        if (!template.IsActive)
            throw new DomainException(NotificationErrors.TemplateInactive(template.Name));
 
        var (subject, body) = template.Render(variables);
 
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            RecipientId = recipientId,
            RecipientType = recipientType,
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone,
            Channel = template.Channel,
            Category = template.Category,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            Priority = priority,
            ReferenceType = referenceType,
            ReferenceId = referenceId
        };
 
        notification.Raise(new NotificationCreatedEvent(
            notification.Id, notification.Channel.ToString(),
            notification.Category.ToString(), recipientId));
 
        return notification;
    }
 
    // ─── Factory: Direct (no template) ──────────────────────
    public static Notification CreateDirect(
        NotificationChannel channel,
        NotificationCategory category,
        string body,
        Guid? recipientId,
        RecipientType recipientType,
        string? subject = null,
        string? recipientEmail = null,
        NotificationPriority priority = NotificationPriority.Normal,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException(NotificationErrors.EmptyBody());
 
        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            RecipientType = recipientType,
            RecipientEmail = recipientEmail,
            Channel = channel,
            Category = category,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            Priority = priority,
            ReferenceType = referenceType,
            ReferenceId = referenceId
        };
    }
 
    // ─── Status Transitions ─────────────────────────────────
    public void MarkQueued()
    {
        if (Status != NotificationStatus.Pending)
            throw new DomainException(NotificationErrors.InvalidStatusTransition(Status, NotificationStatus.Queued));
        Status = NotificationStatus.Queued;
        Touch();
    }
 
    public void MarkSending()
    {
        Status = NotificationStatus.Sending;
        Touch();
    }
 
    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        Touch();
        Raise(new NotificationSentEvent(Id, Channel.ToString(), RecipientId));
    }
 
    public void MarkDelivered()
    {
        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        Touch();
    }
 
    public void MarkRead()
    {
        if (Status != NotificationStatus.Delivered && Status != NotificationStatus.Sent)
            return; // Silently ignore if already read or not yet delivered
        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
        Touch();
    }
 
    public void MarkFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        RetryCount++;
        Touch();
        Raise(new NotificationFailedEvent(Id, Channel.ToString(), reason, RetryCount));
    }
 
    public void Cancel()
    {
        if (Status is NotificationStatus.Sent or NotificationStatus.Delivered or NotificationStatus.Read)
            throw new DomainException(NotificationErrors.CannotCancelSent());
        Status = NotificationStatus.Cancelled;
        Touch();
    }
 
    public bool CanRetry => Status == NotificationStatus.Failed && RetryCount < 3;
 
    public void Retry()
    {
        if (!CanRetry)
            throw new DomainException(NotificationErrors.MaxRetriesExceeded());
        Status = NotificationStatus.Pending;
        FailureReason = null;
        FailedAt = null;
        Touch();
    }
}