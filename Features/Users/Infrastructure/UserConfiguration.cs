using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Domain.ValueObjects;

namespace RetailStore.Api.Features.Users.Infrastructure;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();

        // Email value object -> stored as string column
        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => new Email(v))
            .HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        // PasswordHash value object -> two columns
        builder.OwnsOne(u => u.PasswordHash, ph =>
        {
            ph.Property(p => p.Hash).HasColumnName("PasswordHash").HasMaxLength(128).IsRequired();
            ph.Property(p => p.Salt).HasColumnName("PasswordSalt").HasMaxLength(64).IsRequired();
        });

        builder.Property(u => u.RefreshTokenHash).HasMaxLength(128);
        builder.Property(u => u.Version).IsConcurrencyToken();

        // Role IDs stored as JSON array
        builder.Property(u => u.RoleIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.Ignore(u => u.DomainEvents);
    }
}