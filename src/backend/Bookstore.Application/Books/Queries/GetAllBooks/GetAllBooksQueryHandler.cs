using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Mappers;
using Bookstore.Application.Books.Specifications;
using Bookstore.SharedKernel.Pagination;
using Bookstore.SharedKernel.Results;
using Bookstore.SharedKernel.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Handles retrieval of a page of books, returning items alongside pagination metadata.
/// </summary>
internal sealed class GetAllBooksQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllBooksQuery, Result<PagedResult<BookDto>>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Fetches a page of books from the data store and wraps them with pagination metadata.
    /// </summary>
    /// <remarks>
    /// Queries without change tracking for better read performance. Total count is fetched
    /// with a separate query so callers can render full pagination controls.
    /// </remarks>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing a <see cref="PagedResult{T}"/> of <see cref="BookDto"/> objects.</returns>
    public async ValueTask<Result<PagedResult<BookDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        var specification = new PaginatedBooksSpecification(query.Page, query.PageSize);

        var source = _context.Books.AsNoTracking();

        var totalCount = await source.CountAsync(cancellationToken);

        var books = await SpecificationEvaluator
            .Apply(source, specification)
            .ToListAsync(cancellationToken);

        var items = (IReadOnlyList<BookDto>)[.. books.Select(b => b.ToDto())];

        return Result.Success(new PagedResult<BookDto>(items, totalCount, query.Page, query.PageSize));
    }
}
