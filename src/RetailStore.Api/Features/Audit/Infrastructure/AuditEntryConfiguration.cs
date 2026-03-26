using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Audit.Domain;
 
namespace RetailStore.Api.Features.Audit.Infrastructure;
 
public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditLog", "audit");
        builder.HasKey(a => a.Id);
 
        builder.Property(a => a.UserId);
        builder.Property(a => a.Username).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
 
        builder.Property(a => a.Action).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Module).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(500);
 
        builder.Property(a => a.RequestPayload).HasColumnType("nvarchar(max)");
        builder.Property(a => a.ResponseSummary).HasMaxLength(500);
        builder.Property(a => a.ChangedProperties).HasColumnType("nvarchar(max)");
 
        builder.Property(a => a.Outcome).HasMaxLength(20).IsRequired().HasDefaultValue("Success");
        builder.Property(a => a.ErrorCode).HasMaxLength(50);
        builder.Property(a => a.ErrorMessage).HasMaxLength(500);
        builder.Property(a => a.DurationMs).IsRequired().HasDefaultValue(0);
 
        builder.Property(a => a.CorrelationId).HasMaxLength(50);
        builder.Property(a => a.RequestId).HasMaxLength(50);
        builder.Property(a => a.Timestamp).IsRequired();
 
        builder.HasIndex(a => new { a.UserId, a.Timestamp });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => new { a.Module, a.Timestamp });
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.CorrelationId);
    }
}