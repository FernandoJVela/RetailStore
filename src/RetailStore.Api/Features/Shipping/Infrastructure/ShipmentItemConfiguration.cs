using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Shipping.Domain;
 
namespace RetailStore.Api.Features.Shipping.Infrastructure;
 
public sealed class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems", "shipping");
        builder.HasKey(i => i.Id);
 
        builder.Property(i => i.ShipmentId).IsRequired();
        builder.HasIndex(i => i.ShipmentId);
 
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.WeightKg).HasPrecision(10, 3);
 
        builder.Ignore(i => i.DomainEvents);
    }
}