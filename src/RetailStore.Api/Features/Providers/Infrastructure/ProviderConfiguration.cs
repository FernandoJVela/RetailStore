using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Providers.Domain;
 
namespace RetailStore.Api.Features.Providers.Infrastructure;
 
public sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers", "providers");
        builder.HasKey(p => p.Id);
 
        builder.Property(p => p.CompanyName)
            .HasMaxLength(200)
            .IsRequired();
 
        builder.Property(p => p.ContactName)
            .HasMaxLength(200)
            .IsRequired();
 
        builder.Property(p => p.Email)
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(p => p.Email).IsUnique();  // UQ_Providers_Email
 
        builder.Property(p => p.Phone)
            .HasMaxLength(20);
 
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
 
        // ProductIds stored as JSON string: nvarchar(max)
        builder.Property(p => p.ProductIds)
            .HasColumnName("ProductIds")
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("[]");
 
        builder.Property(p => p.Version).IsConcurrencyToken();
 
        // Indexes
        builder.HasIndex(p => p.CompanyName);
 
        // Ignore computed properties
        builder.Ignore(p => p.ProductIdList);
        builder.Ignore(p => p.ProductCount);
        builder.Ignore(p => p.DomainEvents);
    }
}