using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Products.Domain;
 
namespace RetailStore.Api.Features.Products.Infrastructure;
 
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "products");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
 
        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();
 
        builder.Property(p => p.Description)
            .HasColumnType("nvarchar(max)");
 
        builder.Property(p => p.Category)
            .HasMaxLength(100)
            .IsRequired();
 
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
 
        builder.Property(p => p.Version).IsConcurrencyToken();
 
        // Money as ComplexProperty (not OwnsOne — avoids Added/Modified tracking conflict)
        builder.ComplexProperty(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();
 
            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });
 
        // Indexes for common query patterns
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.IsActive);
 
        builder.Ignore(p => p.DomainEvents);
    }
}