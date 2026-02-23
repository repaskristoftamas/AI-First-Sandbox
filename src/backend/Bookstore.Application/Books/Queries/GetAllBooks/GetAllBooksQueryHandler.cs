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
    /// Fetches all books from the data store without change tracking and maps them to DTOs.
    /// </summary>
    public async ValueTask<Result<IReadOnlyList<BookDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        var books = await _context.Books
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BookDto>>([.. books.Select(b => b.ToDto())]);
    }
}
