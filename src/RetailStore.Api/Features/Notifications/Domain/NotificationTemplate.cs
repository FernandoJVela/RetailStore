using System.Text.RegularExpressions;
using RetailStore.SharedKernel.Domain;
 
namespace RetailStore.Api.Features.Notifications.Domain;
 
public sealed class NotificationTemplate : AggregateRoot
{
    private static readonly Regex PlaceholderPattern = new(
        @"\{\{(\w+)\}\}", RegexOptions.Compiled);
 
    public string Name { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public string? Subject { get; private set; }
    public string BodyTemplate { get; private set; } = string.Empty;
    public NotificationCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;
 
    private NotificationTemplate() { } // EF Core
 
    // ─── Computed ───────────────────────────────────────────
    public List<string> Placeholders =>
        PlaceholderPattern.Matches(BodyTemplate)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();
 
    // ─── Factory ────────────────────────────────────────────
    public static NotificationTemplate Create(
        string name, NotificationChannel channel,
        string bodyTemplate, NotificationCategory category,
        string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(NotificationErrors.InvalidTemplateName());
        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new DomainException(NotificationErrors.InvalidTemplateBody());
 
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Channel = channel,
            Subject = subject?.Trim(),
            BodyTemplate = bodyTemplate,
            Category = category
        };
    }
 
    // ─── Render ─────────────────────────────────────────────
    public (string? Subject, string Body) Render(Dictionary<string, string> variables)
    {
        var body = PlaceholderPattern.Replace(BodyTemplate, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
 
        var subject = Subject is not null
            ? PlaceholderPattern.Replace(Subject, match =>
            {
                var key = match.Groups[1].Value;
                return variables.TryGetValue(key, out var value) ? value : match.Value;
            })
            : null;
 
        return (subject, body);
    }
 
    public void Update(string bodyTemplate, string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new DomainException(NotificationErrors.InvalidTemplateBody());
        BodyTemplate = bodyTemplate;
        Subject = subject?.Trim();
        Touch();
        IncrementVersion();
    }
 
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
}