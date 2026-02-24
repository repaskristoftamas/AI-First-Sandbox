using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Queries.GetBookById;

/// <summary>
/// Query to retrieve a single book by its identifier.
/// </summary>
public sealed record GetBookByIdQuery(BookId Id) : IQuery<Result<BookDto>>;
