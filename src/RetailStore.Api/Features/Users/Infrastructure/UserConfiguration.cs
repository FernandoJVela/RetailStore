using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        // Role IDs stored as JSON array.
        // ValueComparer is REQUIRED so EF Core detects list mutations
        // (e.g. AssignRole / RevokeRole) — without it, _roleIds.Add(...) is
        // not noticed by the change tracker and never persisted.
        var roleIdsComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (a, b) => (a == null && b == null) ||
                      (a != null && b != null && a.SequenceEqual(b)),
            c => c == null
                ? 0
                : c.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
            c => (IReadOnlyCollection<Guid>)c.ToList());

        builder.Property(u => u.RoleIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(roleIdsComparer);

        builder.Ignore(u => u.DomainEvents);
    }
}