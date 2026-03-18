using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Orders.Domain;

namespace RetailStore.Api.Features.Orders.Infrastructure;

public class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();


        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Version)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(o => o.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.Ignore(o => o.DomainEvents);
    }
}
