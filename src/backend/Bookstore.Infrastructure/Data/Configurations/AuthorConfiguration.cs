using Bookstore.Domain.Authors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Author"/> entity.
/// </summary>
/// <remarks>
/// Defines schema constraints and value conversions for the Author table.
/// </remarks>
internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    /// <summary>
    /// Configures the Author table schema.
    /// </summary>
    /// <remarks>
    /// Sets up the strongly-typed identifier conversion and column constraints.
    /// </remarks>
    /// <param name="builder">The builder used to configure the <see cref="Author"/> entity.</param>
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        var converter = new ValueConverter<AuthorId, Guid>(
            id => id.Value,
            guid => new AuthorId(guid));

        var comparer = new ValueComparer<AuthorId>(
            (a, b) => a.Value == b.Value,
            id => id.Value.GetHashCode(),
            id => new AuthorId(id.Value));

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(converter, comparer);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DeletedAt);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
