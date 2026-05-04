using Bookstore.Application.Abstractions;
using Bookstore.Application.Authors.DTOs;
using Bookstore.Application.Authors.Mappers;
using Bookstore.Application.Authors.Specifications;
using Bookstore.SharedKernel.Pagination;
using Bookstore.SharedKernel.Results;
using Bookstore.SharedKernel.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Queries.GetAllAuthors;

/// <summary>
/// Handles retrieval of a page of authors, returning items alongside pagination metadata.
/// </summary>
internal sealed class GetAllAuthorsQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllAuthorsQuery, Result<PagedResult<AuthorDto>>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Fetches a page of authors from the data store and wraps them with pagination metadata.
    /// </summary>
    /// <remarks>
    /// Queries without change tracking for better read performance. Total count is fetched
    /// with a separate query so callers can render full pagination controls.
    /// </remarks>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing a <see cref="PagedResult{T}"/> of <see cref="AuthorDto"/> objects.</returns>
    public async ValueTask<Result<PagedResult<AuthorDto>>> Handle(GetAllAuthorsQuery query, CancellationToken cancellationToken)
    {
        var specification = new PaginatedAuthorsSpecification(query.Page, query.PageSize);

        var source = _context.Authors.AsNoTracking();

        var totalCount = await source.CountAsync(cancellationToken);

        var authors = await SpecificationEvaluator
            .Apply(source, specification)
            .ToListAsync(cancellationToken);

        var items = (IReadOnlyList<AuthorDto>)[.. authors.Select(a => a.ToDto())];

        return Result.Success(new PagedResult<AuthorDto>(items, totalCount, query.Page, query.PageSize));
    }
}
