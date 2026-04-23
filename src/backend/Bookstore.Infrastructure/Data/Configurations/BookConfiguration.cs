using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Book"/> entity.
/// </summary>
/// <remarks>
/// Defines schema constraints and value conversions for the Book table.
/// </remarks>
internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    /// <summary>
    /// Configures the Book table schema.
    /// </summary>
    /// <remarks>
    /// Sets up the strongly-typed identifier conversion, column constraints, and a unique index on ISBN.
    /// </remarks>
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

        var authorIdConverter = new ValueConverter<AuthorId, Guid>(
            id => id.Value,
            guid => new AuthorId(guid));

        var authorIdComparer = new ValueComparer<AuthorId>(
            (a, b) => a.Value == b.Value,
            id => id.Value.GetHashCode(),
            id => new AuthorId(id.Value));

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.AuthorId)
            .HasConversion(authorIdConverter, authorIdComparer)
            .IsRequired();

        builder.HasOne<Author>()
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        var isbnConverter = new ValueConverter<Isbn, string>(
            isbn => isbn.Value,
            value => Isbn.FromDatabase(value));

        builder.Property(b => b.ISBN)
            .HasConversion(isbnConverter)
            .IsRequired()
            .HasMaxLength(13);

        builder.HasIndex(b => b.ISBN)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(b => b.Price)
            .HasPrecision(10, 2);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAt);

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
