using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Customers.Domain;
 
namespace RetailStore.Api.Features.Customers.Infrastructure;
 
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "customers");
        builder.HasKey(c => c.Id);
 
        builder.Property(c => c.FirstName)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(c => c.LastName)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(c => c.Email)
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();
 
        builder.Property(c => c.Phone)
            .HasMaxLength(20);
 
        // Shipping Address as flat columns (matches your DB schema exactly)
        builder.Property(c => c.ShippingStreet).HasMaxLength(300);
        builder.Property(c => c.ShippingCity).HasMaxLength(100);
        builder.Property(c => c.ShippingState).HasMaxLength(100);
        builder.Property(c => c.ShippingZipCode).HasMaxLength(20);
        builder.Property(c => c.ShippingCountry).HasMaxLength(100);
 
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
 
        builder.Property(c => c.Version).IsConcurrencyToken();
 
        // Indexes for common queries
        builder.HasIndex(c => new { c.LastName, c.FirstName });
        builder.HasIndex(c => c.IsActive);
 
        // Ignore computed properties
        builder.Ignore(c => c.FullName);
        builder.Ignore(c => c.ShippingAddress);
        builder.Ignore(c => c.DomainEvents);
    }
}