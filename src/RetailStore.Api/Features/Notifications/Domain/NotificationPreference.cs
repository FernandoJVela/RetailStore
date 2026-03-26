using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Domain;
 
public sealed class NotificationPreference : Entity
{
    public Guid RecipientId { get; private set; }
    public RecipientType RecipientType { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsEnabled { get; private set; } = true;
 
    private NotificationPreference() { } // EF Core
 
    public static NotificationPreference Create(
        Guid recipientId, RecipientType recipientType,
        NotificationCategory category, NotificationChannel channel,
        bool isEnabled = true)
    {
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            RecipientType = recipientType,
            Category = category,
            Channel = channel,
            IsEnabled = isEnabled
        };
    }
 
    public void Enable() { IsEnabled = true; Touch(); }
    public void Disable() { IsEnabled = false; Touch(); }
    public void Toggle() { IsEnabled = !IsEnabled; Touch(); }
}