using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Orders.Domain;

namespace RetailStore.Api.Features.Orders.Infrastructure;

public class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderLineItems", "orders");


        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderId)
            .IsRequired();

        builder.Property(o => o.ProductId)
            .IsRequired();

        builder.Property(o => o.Quantity)
            .IsRequired();

        builder.Property(o => o.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.UnitPriceCurrency)
            .HasColumnType("nvarchar(3)")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnType("datetime");

        builder.Ignore(o => o.DomainEvents);
    }
}
