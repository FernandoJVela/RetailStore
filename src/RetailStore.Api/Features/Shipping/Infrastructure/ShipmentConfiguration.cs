using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Shipping.Domain;
 
namespace RetailStore.Api.Features.Shipping.Infrastructure;
 
public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments", "shipping");
        builder.HasKey(s => s.Id);
 
        builder.Property(s => s.OrderId).IsRequired();
        builder.HasIndex(s => s.OrderId).IsUnique();  // UQ_Shipments_OrderId
 
        builder.Property(s => s.CustomerId).IsRequired();
        builder.HasIndex(s => s.CustomerId);
 
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(s => s.Status);
 
        builder.Property(s => s.Carrier).HasMaxLength(100);
        builder.Property(s => s.TrackingNumber).HasMaxLength(100);
        builder.HasIndex(s => s.TrackingNumber);
        builder.Property(s => s.EstimatedDelivery);
 
        // Address
        builder.Property(s => s.Street).HasMaxLength(300).IsRequired();
        builder.Property(s => s.City).HasMaxLength(100).IsRequired();
        builder.Property(s => s.State).HasMaxLength(100);
        builder.Property(s => s.ZipCode).HasMaxLength(20);
        builder.Property(s => s.Country).HasMaxLength(100).IsRequired();
 
        // Cost
        builder.Property(s => s.ShippingCost).HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(s => s.CostCurrency).HasMaxLength(3).IsRequired().HasDefaultValue("USD");
 
        // Weight
        builder.Property(s => s.TotalWeightKg).HasPrecision(10, 3);
 
        // Timestamps
        builder.Property(s => s.ShippedAt);
        builder.Property(s => s.DeliveredAt);
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.Version).IsConcurrencyToken();
 
        // Items relationship
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.Navigation(s => s.Items).AutoInclude();
 
        // Ignore computed
        builder.Ignore(s => s.FullAddress);
        builder.Ignore(s => s.ItemCount);
        builder.Ignore(s => s.TotalQuantity);
        builder.Ignore(s => s.DomainEvents);
    }
}