using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Orders.Domain;
 
namespace RetailStore.Api.Features.Orders.Infrastructure;
 
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "orders");
        builder.HasKey(o => o.Id);
 
        builder.Property(o => o.CustomerId).IsRequired();
 
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
 
        builder.Property(o => o.OrderDate)
            .HasColumnType("datetime2(7)")
            .IsRequired();
 
        builder.Property(o => o.CompletedAt)
            .HasColumnType("datetime2(7)");
 
        builder.Property(o => o.CancelledAt)
            .HasColumnType("datetime2(7)");
 
        builder.Property(o => o.Version).IsConcurrencyToken();
 
        // Relationship: Order → OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(li => li.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
 
        builder.Navigation(o => o.Items).AutoInclude();
 
        // Ignore computed properties not in DB
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.Total);
        builder.Ignore(o => o.DomainEvents);
    }
}