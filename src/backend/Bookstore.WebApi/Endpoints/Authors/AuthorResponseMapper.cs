using Bookstore.Application.Authors.DTOs;
using Riok.Mapperly.Abstractions;

namespace Bookstore.WebApi.Endpoints.Authors;

/// <summary>
/// Converts <see cref="AuthorDto"/> objects to <see cref="AuthorResponse"/> models.
/// </summary>
/// <remarks>
/// Implementation is generated at compile time by Mapperly.
/// </remarks>
[Mapper]
internal static partial class AuthorResponseMapper
{
    /// <summary>
    /// Maps an <see cref="AuthorDto"/> to an API response model.
    /// </summary>
    /// <param name="dto">The DTO to convert.</param>
    /// <returns>An <see cref="AuthorResponse"/> containing the mapped values.</returns>
    public static partial AuthorResponse ToResponse(this AuthorDto dto);
}
