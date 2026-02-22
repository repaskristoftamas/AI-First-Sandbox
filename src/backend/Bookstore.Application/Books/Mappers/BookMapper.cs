using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Books;
using Riok.Mapperly.Abstractions;

namespace Bookstore.Application.Books.Mappers;

[Mapper]
internal static partial class BookMapper
{
    public static partial BookDto ToDto(this Book book);

    private static Guid MapBookId(BookId id) => id.Value;
}
