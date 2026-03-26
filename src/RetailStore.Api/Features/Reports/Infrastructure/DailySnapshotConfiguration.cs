using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Reports.Domain;
 
namespace RetailStore.Api.Features.Reports.Infrastructure;
 
public sealed class DailySalesSnapshotConfiguration : IEntityTypeConfiguration<DailySalesSnapshot>
{
    public void Configure(EntityTypeBuilder<DailySalesSnapshot> builder)
    {
        builder.ToTable("DailySalesSnapshots", "reports");
        builder.HasKey(s => s.Id);
 
        builder.Property(s => s.Date).IsRequired();
        builder.HasIndex(s => s.Date).IsUnique();
 
        builder.Property(s => s.TotalOrders).IsRequired();
        builder.Property(s => s.ConfirmedOrders).IsRequired();
        builder.Property(s => s.CancelledOrders).IsRequired();
        builder.Property(s => s.CompletedOrders).IsRequired();
        builder.Property(s => s.TotalRevenue).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.TotalItemsSold).IsRequired();
        builder.Property(s => s.AverageOrderValue).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.UniqueCustomers).IsRequired();
        builder.Property(s => s.NewCustomers).IsRequired();
        builder.Property(s => s.TotalPaymentsCaptured).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.TotalRefunds).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.TopCategory).HasMaxLength(100);
    }
}