using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Handles retrieval of all books, returning them as a read-only list of DTOs.
/// </summary>
internal sealed class GetAllBooksQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllBooksQuery, Result<IReadOnlyList<BookDto>>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Fetches a page of books from the data store and maps them to DTOs.
    /// </summary>
    /// <remarks>
    /// Queries without change tracking for better read performance.
    /// </remarks>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing a read-only list of <see cref="BookDto"/> objects.</returns>
    public async ValueTask<Result<IReadOnlyList<BookDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        //TODO: order by different fields, filter by author, publication year, etc.
        var books = await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.Id.Value)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BookDto>>([.. books.Select(b => b.ToDto())]);
    }
}
