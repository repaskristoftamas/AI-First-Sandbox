using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Riok.Mapperly.Abstractions;

namespace Bookstore.Application.Books.Mappers;

/// <summary>
/// Converts <see cref="Book"/> domain entities to <see cref="BookDto"/> objects.
/// </summary>
/// <remarks>
/// Implementation is generated at compile time by Mapperly.
/// </remarks>
[Mapper]
internal static partial class BookMapper
{
    /// <summary>
    /// Maps a <see cref="Book"/> entity to its DTO representation.
    /// </summary>
    /// <param name="book">The domain entity to convert.</param>
    /// <returns>A <see cref="BookDto"/> containing the mapped values.</returns>
    [MapperIgnoreSource(nameof(Book.DomainEvents))]
    public static partial BookDto ToDto(this Book book);

    /// <summary>
    /// Extracts the underlying <see cref="Guid"/> from a <see cref="BookId"/> for DTO mapping.
    /// </summary>
    /// <param name="id">The strongly-typed book identifier.</param>
    /// <returns>The underlying <see cref="Guid"/> value.</returns>
    private static Guid MapBookId(BookId id) => id.Value;

    /// <summary>
    /// Extracts the underlying <see cref="Guid"/> from an <see cref="AuthorId"/> for DTO mapping.
    /// </summary>
    /// <param name="id">The strongly-typed author identifier.</param>
    /// <returns>The underlying <see cref="Guid"/> value.</returns>
    private static Guid MapAuthorId(AuthorId id) => id.Value;
}
