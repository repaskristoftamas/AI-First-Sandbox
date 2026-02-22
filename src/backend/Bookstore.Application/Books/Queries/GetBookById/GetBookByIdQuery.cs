using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Queries.GetBookById;

public sealed record GetBookByIdQuery(BookId Id) : IQuery<Result<BookDto>>;
