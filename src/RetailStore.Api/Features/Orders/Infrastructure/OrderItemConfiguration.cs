using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Orders.Domain;
 
namespace RetailStore.Api.Features.Orders.Infrastructure;
 
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderLineItems", "orders");
        builder.HasKey(li => li.Id);
 
        builder.Property(li => li.OrderId).IsRequired();
        builder.Property(li => li.ProductId).IsRequired();
        builder.Property(li => li.Quantity).IsRequired();
 
        builder.Property(li => li.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasPrecision(18, 2)
            .IsRequired();
 
        builder.Property(li => li.UnitPriceCurrency)
            .HasColumnName("UnitPriceCurrency")
            .HasMaxLength(3)
            .IsRequired();
 
        // Ignore computed properties not in DB
        builder.Ignore(li => li.Subtotal);
        builder.Ignore(li => li.UnitPriceMoney);
        builder.Ignore(li => li.SubtotalMoney);
        builder.Ignore(li => li.DomainEvents);
    }
}
