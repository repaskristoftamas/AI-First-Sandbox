using Bookstore.Application.Books.DTOs;
using Riok.Mapperly.Abstractions;

namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// Mapperly-generated mapper that converts <see cref="BookDto"/> objects to <see cref="BookResponse"/> models.
/// </summary>
[Mapper]
internal static partial class BookResponseMapper
{
    /// <summary>
    /// Maps a <see cref="BookDto"/> to an API response model.
    /// </summary>
    /// <param name="dto">The DTO to convert.</param>
    /// <returns>A <see cref="BookResponse"/> containing the mapped values.</returns>
    public static partial BookResponse ToResponse(this BookDto dto);
}
