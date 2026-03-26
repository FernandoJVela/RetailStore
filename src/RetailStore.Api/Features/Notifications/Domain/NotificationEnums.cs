namespace RetailStore.Api.Features.Notifications.Domain;
 
public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    InApp
}
 
public enum NotificationStatus
{
    Pending,
    Queued,
    Sending,
    Sent,
    Delivered,
    Read,
    Failed,
    Cancelled
}
 
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}
 
public enum NotificationCategory
{
    Order,
    Inventory,
    Shipping,
    User,
    System,
    Marketing
}
 
public enum RecipientType
{
    User,
    Customer,
    System
}