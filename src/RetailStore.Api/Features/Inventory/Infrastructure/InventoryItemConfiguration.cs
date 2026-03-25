using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Inventory.Domain;
 
namespace RetailStore.Api.Features.Inventory.Infrastructure;
 
public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems", "inventory");
        builder.HasKey(i => i.Id);
 
        builder.Property(i => i.ProductId).IsRequired();
        builder.HasIndex(i => i.ProductId).IsUnique(); // UQ_InventoryItems_ProductId
 
        builder.Property(i => i.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(0);
 
        builder.Property(i => i.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);
 
        builder.Property(i => i.ReorderThreshold)
            .IsRequired()
            .HasDefaultValue(10);
 
        builder.Property(i => i.Version).IsConcurrencyToken();
 
        // Ignore computed properties
        builder.Ignore(i => i.AvailableQuantity);
        builder.Ignore(i => i.IsLowStock);
        builder.Ignore(i => i.IsOutOfStock);
        builder.Ignore(i => i.StockStatus);
        builder.Ignore(i => i.DomainEvents);
    }
}