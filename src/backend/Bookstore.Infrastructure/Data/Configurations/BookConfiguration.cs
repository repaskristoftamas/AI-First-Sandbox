using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Book"/> entity, defining schema constraints and value conversions.
/// </summary>
internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    /// <summary>
    /// Configures the Book table schema including the strongly-typed identifier conversion, column constraints, and unique index on ISBN.
    /// </summary>
    /// <param name="builder">The builder used to configure the <see cref="Book"/> entity.</param>
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        var converter = new ValueConverter<BookId, Guid>(
            id => id.Value,
            guid => new BookId(guid));

        var comparer = new ValueComparer<BookId>(
            (a, b) => a.Value == b.Value,
            id => id.Value.GetHashCode(),
            id => new BookId(id.Value));

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(converter, comparer);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(13);

        builder.HasIndex(b => b.ISBN)
            .IsUnique();

        builder.Property(b => b.Price)
            .HasPrecision(10, 2);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);
    }
}
