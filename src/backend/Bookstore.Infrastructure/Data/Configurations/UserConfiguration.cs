using Bookstore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="User"/> entity.
/// </summary>
/// <remarks>
/// Defines schema constraints and value conversions for the Users table.
/// </remarks>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the Users table schema.
    /// </summary>
    /// <remarks>
    /// Sets up the strongly-typed identifier conversion, column constraints, and a unique index on Email.
    /// </remarks>
    /// <param name="builder">The builder used to configure the <see cref="User"/> entity.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var converter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            guid => new UserId(guid));

        var comparer = new ValueComparer<UserId>(
            (a, b) => a.Value == b.Value,
            id => id.Value.GetHashCode(),
            id => new UserId(id.Value));

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(converter, comparer);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.Roles)
            .HasConversion(
                roles => string.Join(',', roles.Select(r => r.ToString())),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Enum.Parse<Role>(s))
                    .ToList(),
                new ValueComparer<IReadOnlyCollection<Role>>(
                    (a, b) => a != null && b != null && a.SequenceEqual(b),
                    c => c.Aggregate(0, (hash, role) => HashCode.Combine(hash, role)),
                    c => c.ToList()))
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);
    }
}
