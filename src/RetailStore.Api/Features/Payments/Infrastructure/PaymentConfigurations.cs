using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Payments.Domain;
 
namespace RetailStore.Api.Features.Payments.Infrastructure;
 
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "payments");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.OrderId).IsRequired();
        builder.HasIndex(p => p.OrderId);
        builder.Property(p => p.CustomerId).IsRequired();
        builder.HasIndex(p => p.CustomerId);
 
        builder.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("USD");
 
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.Method);
        builder.Property(p => p.MethodDetail).HasMaxLength(100);
 
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);
        builder.HasIndex(p => p.Status);
 
        builder.Property(p => p.GatewayName).HasMaxLength(50);
        builder.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        builder.HasIndex(p => p.GatewayTransactionId);
        builder.Property(p => p.GatewayResponse).HasColumnType("nvarchar(max)");
 
        builder.Property(p => p.AuthorizedAt);
        builder.Property(p => p.CapturedAt);
        builder.Property(p => p.FailedAt);
        builder.Property(p => p.RefundedAt);
        builder.Property(p => p.CancelledAt);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.Version).IsConcurrencyToken();
 
        builder.HasMany(p => p.Refunds)
            .WithOne()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);
 
        builder.Navigation(p => p.Refunds).AutoInclude();
 
        builder.Ignore(p => p.TotalRefunded);
        builder.Ignore(p => p.NetAmount);
        builder.Ignore(p => p.IsFullyRefunded);
        builder.Ignore(p => p.DomainEvents);
    }
}
 
public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds", "payments");
        builder.HasKey(r => r.Id);
 
        builder.Property(r => r.PaymentId).IsRequired();
        builder.HasIndex(r => r.PaymentId);
 
        builder.Property(r => r.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.Currency).HasMaxLength(3).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired()
            .HasDefaultValue(RefundStatus.Pending);
        builder.Property(r => r.GatewayRefundId).HasMaxLength(200);
        builder.Property(r => r.ProcessedAt);
        builder.Property(r => r.FailedAt);
        builder.Property(r => r.FailureReason).HasMaxLength(500);
 
        builder.Ignore(r => r.DomainEvents);
    }
}