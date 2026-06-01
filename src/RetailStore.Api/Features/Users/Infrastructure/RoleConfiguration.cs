using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetailStore.Api.Features.Users.Domain;
using RetailStore.SharedKernel.Domain;

namespace RetailStore.Api.Features.Users.Infrastructure;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "users");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();

        // Permissions stored as JSON array of "resource:action" strings.
        // ValueComparer is REQUIRED so EF Core detects list mutations
        // (AddPermission / UpdatePermissions) — without it, _permissions.Add(...)
        // is not noticed and never persisted.
        var permComparer = new ValueComparer<IReadOnlyCollection<Permission>>(
            (a, b) => (a == null && b == null) ||
                      (a != null && b != null &&
                       a.Select(p => p.FullName).SequenceEqual(b.Select(p => p.FullName))),
            c => c == null
                ? 0
                : c.Aggregate(0, (h, v) => HashCode.Combine(h, v.FullName.GetHashCode())),
            c => (IReadOnlyCollection<Permission>)c.ToList());

        builder.Property(r => r.Permissions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(
                    v.Select(p => p.FullName).ToList(), (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                    v, (System.Text.Json.JsonSerializerOptions?)null)!
                    .Select(s => Permission.Parse(s)).ToList())
            .Metadata.SetValueComparer(permComparer);

        builder.Property(r => r.Version).IsConcurrencyToken();
        builder.Ignore(r => r.DomainEvents);
    }
}