using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Domain;
 
public static class NotificationErrors
{
    public static DomainError NotFound(Guid id) => new(
        "NOTIFICATION_NOT_FOUND", $"Notification '{id}' not found.", DomainErrorType.NotFound);
    public static DomainError TemplateNotFound(string name, string channel) => new(
        "NOTIFICATION_TEMPLATE_NOT_FOUND", $"Template '{name}' for channel '{channel}' not found.", DomainErrorType.NotFound);
    public static DomainError TemplateInactive(string name) => new(
        "NOTIFICATION_TEMPLATE_INACTIVE", $"Template '{name}' is inactive.", DomainErrorType.BusinessRule);
    public static DomainError InvalidTemplateName() => new(
        "NOTIFICATION_INVALID_TEMPLATE_NAME", "Template name is required.", DomainErrorType.Validation);
    public static DomainError InvalidTemplateBody() => new(
        "NOTIFICATION_INVALID_TEMPLATE_BODY", "Template body is required.", DomainErrorType.Validation);
    public static DomainError EmptyBody() => new(
        "NOTIFICATION_EMPTY_BODY", "Notification body cannot be empty.", DomainErrorType.Validation);
    public static DomainError InvalidStatusTransition(NotificationStatus from, NotificationStatus to) => new(
        "NOTIFICATION_INVALID_STATUS", $"Cannot transition from '{from}' to '{to}'.", DomainErrorType.BusinessRule);
    public static DomainError CannotCancelSent() => new(
        "NOTIFICATION_CANNOT_CANCEL_SENT", "Cannot cancel a notification that has already been sent.", DomainErrorType.BusinessRule);
    public static DomainError MaxRetriesExceeded() => new(
        "NOTIFICATION_MAX_RETRIES", "Maximum retry attempts (3) exceeded.", DomainErrorType.BusinessRule);
    public static DomainError PreferenceDisabled(string category, string channel) => new(
        "NOTIFICATION_PREFERENCE_DISABLED", $"Recipient has disabled {channel} notifications for {category}.", DomainErrorType.BusinessRule);
    public static DomainError NoRecipientContact() => new(
        "NOTIFICATION_NO_RECIPIENT_CONTACT", "No email or phone for the recipient.", DomainErrorType.Validation);
}