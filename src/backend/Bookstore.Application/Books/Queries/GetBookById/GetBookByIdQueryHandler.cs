using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetBookById;

/// <summary>
/// Handles retrieval of a single book by identifier.
/// </summary>
/// <remarks>
/// Returns a not-found error if the book does not exist.
/// </remarks>
internal sealed class GetBookByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Looks up the book by its identifier and maps it to a DTO.
    /// </summary>
    /// <remarks>
    /// Returns a not-found error if the book does not exist.
    /// </remarks>
    /// <param name="query">The query containing the book identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the <see cref="BookDto"/>, or a <see cref="NotFoundError"/> if not found.</returns>
    public async ValueTask<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);

        if (book is null)
            return Result.Failure<BookDto>(new NotFoundError("The book with the specified identifier was not found."));

        return Result.Success(book.ToDto());
    }
}
