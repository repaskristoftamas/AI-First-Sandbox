using Bookstore.Application.Books.DTOs;
using Riok.Mapperly.Abstractions;

namespace Bookstore.WebApi.Endpoints.Books;

[Mapper]
internal static partial class BookResponseMapper
{
    public static partial BookResponse ToResponse(this BookDto dto);
}
