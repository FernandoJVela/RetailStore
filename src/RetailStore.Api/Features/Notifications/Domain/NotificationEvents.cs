using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Domain;
 
public sealed record NotificationCreatedEvent(
    Guid NotificationId, string Channel,
    string Category, Guid? RecipientId) : DomainEvent
{ public override string EventType => "NotificationCreated"; }
 
public sealed record NotificationSentEvent(
    Guid NotificationId, string Channel,
    Guid? RecipientId) : DomainEvent
{ public override string EventType => "NotificationSent"; }
 
public sealed record NotificationFailedEvent(
    Guid NotificationId, string Channel,
    string Reason, int RetryCount) : DomainEvent
{ public override string EventType => "NotificationFailed"; }