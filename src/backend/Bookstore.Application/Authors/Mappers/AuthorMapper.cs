using Bookstore.Application.Authors.DTOs;
using Bookstore.Domain.Authors;
using Riok.Mapperly.Abstractions;

namespace Bookstore.Application.Authors.Mappers;

/// <summary>
/// Converts <see cref="Author"/> domain entities to <see cref="AuthorDto"/> objects.
/// </summary>
/// <remarks>
/// Implementation is generated at compile time by Mapperly.
/// </remarks>
[Mapper]
internal static partial class AuthorMapper
{
    /// <summary>
    /// Maps an <see cref="Author"/> entity to its DTO representation.
    /// </summary>
    /// <param name="author">The domain entity to convert.</param>
    /// <returns>An <see cref="AuthorDto"/> containing the mapped values.</returns>
    [MapperIgnoreSource(nameof(Author.Books))]
    public static partial AuthorDto ToDto(this Author author);

    /// <summary>
    /// Extracts the underlying <see cref="Guid"/> from an <see cref="AuthorId"/> for DTO mapping.
    /// </summary>
    /// <param name="id">The strongly-typed author identifier.</param>
    /// <returns>The underlying <see cref="Guid"/> value.</returns>
    private static Guid MapAuthorId(AuthorId id) => id.Value;
}
