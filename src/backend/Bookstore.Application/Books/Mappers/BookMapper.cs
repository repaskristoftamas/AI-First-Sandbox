using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Books;
using Riok.Mapperly.Abstractions;

namespace Bookstore.Application.Books.Mappers;

/// <summary>
/// Mapperly-generated mapper that converts <see cref="Book"/> domain entities to <see cref="BookDto"/> objects.
/// </summary>
[Mapper]
internal static partial class BookMapper
{
    /// <summary>
    /// Maps a <see cref="Book"/> entity to its DTO representation.
    /// </summary>
    public static partial BookDto ToDto(this Book book);

    /// <summary>
    /// Extracts the underlying <see cref="Guid"/> from a <see cref="BookId"/> for DTO mapping.
    /// </summary>
    private static Guid MapBookId(BookId id) => id.Value;
}
