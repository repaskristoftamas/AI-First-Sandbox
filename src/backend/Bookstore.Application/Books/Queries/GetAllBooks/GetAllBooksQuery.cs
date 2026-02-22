using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

public sealed record GetAllBooksQuery : IQuery<Result<IReadOnlyList<BookDto>>>;
