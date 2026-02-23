using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Query to retrieve all books in the catalog.
/// </summary>
public sealed record GetAllBooksQuery : IQuery<Result<IReadOnlyList<BookDto>>>;
