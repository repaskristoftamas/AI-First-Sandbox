using Bookstore.Application.Abstractions;
using Bookstore.Application.Authors.DTOs;
using Bookstore.Application.Authors.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Queries.GetAllAuthors;

/// <summary>
/// Handles retrieval of all authors, returning them as a read-only list of DTOs.
/// </summary>
internal sealed class GetAllAuthorsQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllAuthorsQuery, Result<IReadOnlyList<AuthorDto>>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Fetches all authors from the data store and maps them to DTOs.
    /// </summary>
    /// <remarks>
    /// Queries without change tracking for better read performance.
    /// </remarks>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing a read-only list of <see cref="AuthorDto"/> objects.</returns>
    public async ValueTask<Result<IReadOnlyList<AuthorDto>>> Handle(GetAllAuthorsQuery query, CancellationToken cancellationToken)
    {
        var authors = await _context.Authors
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AuthorDto>>([.. authors.Select(a => a.ToDto())]);
    }
}
