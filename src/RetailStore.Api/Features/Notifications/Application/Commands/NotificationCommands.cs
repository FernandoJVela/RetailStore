using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RetailStore.Api.Features.Notifications.Domain;
using RetailStore.Infrastructure.Persistence;
using RetailStore.SharedKernel.Application;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Application.Commands;
 
// ═══════════════════════════════════════════════════════════
// SEND NOTIFICATION (using template)
// ═══════════════════════════════════════════════════════════
public sealed record SendNotificationCommand(
    string TemplateName,
    string Channel,
    Guid? RecipientId,
    string RecipientType,
    string? RecipientEmail = null,
    string? RecipientPhone = null,
    string Priority = "Normal",
    Dictionary<string, string>? Variables = null,
    string? ReferenceType = null,
    Guid? ReferenceId = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "notifications:write";
}
 
public sealed class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.TemplateName).NotEmpty();
        RuleFor(x => x.Channel).NotEmpty()
            .Must(c => Enum.TryParse<NotificationChannel>(c, true, out _))
            .WithMessage("Invalid channel. Use: Email, Sms, Push, InApp");
        RuleFor(x => x.RecipientType).NotEmpty()
            .Must(r => Enum.TryParse<Domain.RecipientType>(r, true, out _))
            .WithMessage("Invalid recipient type. Use: User, Customer, System");
    }
}
 
public sealed class SendNotificationHandler(RetailStoreDbContext db)
    : IRequestHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendNotificationCommand cmd, CancellationToken ct)
    {
        var channel = Enum.Parse<NotificationChannel>(cmd.Channel, true);
        var recipientType = Enum.Parse<Domain.RecipientType>(cmd.RecipientType, true);
        var priority = Enum.Parse<NotificationPriority>(cmd.Priority, true);
 
        // Find template
        var template = await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == cmd.TemplateName
                && t.Channel == channel && t.IsActive, ct)
            ?? throw new DomainException(
                NotificationErrors.TemplateNotFound(cmd.TemplateName, cmd.Channel));
 
        // Check recipient preferences
        if (cmd.RecipientId.HasValue)
        {
            var prefDisabled = await db.Set<NotificationPreference>()
                .AnyAsync(p => p.RecipientId == cmd.RecipientId.Value
                    && p.Category == template.Category
                    && p.Channel == channel
                    && !p.IsEnabled, ct);
 
            if (prefDisabled)
                throw new DomainException(
                    NotificationErrors.PreferenceDisabled(
                        template.Category.ToString(), channel.ToString()));
        }
 
        var variables = cmd.Variables ?? new Dictionary<string, string>();
        var notification = Notification.CreateFromTemplate(
            template, variables, cmd.RecipientId, recipientType,
            cmd.RecipientEmail, cmd.RecipientPhone, priority,
            cmd.ReferenceType, cmd.ReferenceId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        return notification.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// SEND DIRECT (no template)
// ═══════════════════════════════════════════════════════════
public sealed record SendDirectNotificationCommand(
    string Channel, string Category, string Body,
    Guid? RecipientId, string RecipientType,
    string? Subject = null, string? RecipientEmail = null,
    string Priority = "Normal",
    string? ReferenceType = null, Guid? ReferenceId = null
) : ICommand<Guid>, IRequirePermission
{
    public string RequiredPermission => "notifications:write";
}
 
public sealed class SendDirectHandler(RetailStoreDbContext db)
    : IRequestHandler<SendDirectNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendDirectNotificationCommand cmd, CancellationToken ct)
    {
        var channel = Enum.Parse<NotificationChannel>(cmd.Channel, true);
        var category = Enum.Parse<NotificationCategory>(cmd.Category, true);
        var recipientType = Enum.Parse<Domain.RecipientType>(cmd.RecipientType, true);
        var priority = Enum.Parse<NotificationPriority>(cmd.Priority, true);
 
        var notification = Notification.CreateDirect(
            channel, category, cmd.Body, cmd.RecipientId, recipientType,
            cmd.Subject, cmd.RecipientEmail, priority,
            cmd.ReferenceType, cmd.ReferenceId);
 
        await db.Set<Notification>().AddAsync(notification, ct);
        return notification.Id;
    }
}
 
// ═══════════════════════════════════════════════════════════
// MARK READ
// ═══════════════════════════════════════════════════════════
public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand;
 
public sealed class MarkReadHandler(RetailStoreDbContext db)
    : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkNotificationReadCommand cmd, CancellationToken ct)
    {
        var notification = await db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == cmd.NotificationId, ct)
            ?? throw new DomainException(NotificationErrors.NotFound(cmd.NotificationId));
        notification.MarkRead();
        return Unit.Value;
    }
}
 
// ═══════════════════════════════════════════════════════════
// UPDATE PREFERENCE
// ═══════════════════════════════════════════════════════════
public sealed record UpdatePreferenceCommand(
    Guid RecipientId, string RecipientType,
    string Category, string Channel, bool IsEnabled
) : ICommand;
 
public sealed class UpdatePreferenceHandler(RetailStoreDbContext db)
    : IRequestHandler<UpdatePreferenceCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePreferenceCommand cmd, CancellationToken ct)
    {
        var category = Enum.Parse<NotificationCategory>(cmd.Category, true);
        var channel = Enum.Parse<NotificationChannel>(cmd.Channel, true);
        var recipientType = Enum.Parse<Domain.RecipientType>(cmd.RecipientType, true);
 
        var pref = await db.Set<NotificationPreference>()
            .FirstOrDefaultAsync(p => p.RecipientId == cmd.RecipientId
                && p.Category == category && p.Channel == channel, ct);
 
        if (pref is null)
        {
            pref = NotificationPreference.Create(
                cmd.RecipientId, recipientType, category, channel, cmd.IsEnabled);
            await db.Set<NotificationPreference>().AddAsync(pref, ct);
        }
        else
        {
            if (cmd.IsEnabled) pref.Enable(); else pref.Disable();
        }
        return Unit.Value;
    }
}