using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Pagination;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Query to retrieve a page of books from the catalog.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of books per page.</param>
public sealed record GetAllBooksQuery(int Page = 1, int PageSize = 20) : IQuery<Result<PagedResult<BookDto>>>;
