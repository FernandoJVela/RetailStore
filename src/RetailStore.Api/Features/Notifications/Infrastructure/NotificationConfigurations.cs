using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Notifications.Domain;
 
namespace RetailStore.Api.Features.Notifications.Infrastructure;
 
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", "notifications");
        builder.HasKey(n => n.Id);
 
        builder.Property(n => n.TemplateId);
        builder.Property(n => n.RecipientId);
        builder.Property(n => n.RecipientType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.RecipientEmail).HasMaxLength(256);
        builder.Property(n => n.RecipientPhone).HasMaxLength(20);
 
        builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(300);
        builder.Property(n => n.Body).HasColumnType("nvarchar(max)").IsRequired();
 
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(NotificationStatus.Pending);
        builder.Property(n => n.Priority).HasConversion<string>().HasMaxLength(10).IsRequired().HasDefaultValue(NotificationPriority.Normal);
 
        builder.Property(n => n.SentAt);
        builder.Property(n => n.DeliveredAt);
        builder.Property(n => n.ReadAt);
        builder.Property(n => n.FailedAt);
        builder.Property(n => n.FailureReason).HasMaxLength(500);
        builder.Property(n => n.RetryCount).HasDefaultValue(0);
 
        builder.Property(n => n.ReferenceType).HasMaxLength(50);
        builder.Property(n => n.ReferenceId);
        builder.Property(n => n.Version).IsConcurrencyToken();
 
        builder.HasIndex(n => new { n.RecipientId, n.RecipientType });
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => new { n.ReferenceType, n.ReferenceId });
        builder.HasIndex(n => new { n.Category, n.CreatedAt });
 
        builder.Ignore(n => n.CanRetry);
        builder.Ignore(n => n.DomainEvents);
    }
}
 
public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates", "notifications");
        builder.HasKey(t => t.Id);
 
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(t => new { t.Name, t.Channel }).IsUnique();
 
        builder.Property(t => t.Subject).HasMaxLength(300);
        builder.Property(t => t.BodyTemplate).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(t => t.IsActive).HasDefaultValue(true);
        builder.Property(t => t.Version).IsConcurrencyToken();
 
        builder.HasIndex(t => t.Category);
 
        builder.Ignore(t => t.Placeholders);
        builder.Ignore(t => t.DomainEvents);
    }
}
 
public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences", "notifications");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.RecipientId).IsRequired();
        builder.Property(p => p.RecipientType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.IsEnabled).HasDefaultValue(true);
 
        builder.HasIndex(p => new { p.RecipientId, p.RecipientType, p.Category, p.Channel }).IsUnique();
 
        builder.Ignore(p => p.DomainEvents);
    }
}