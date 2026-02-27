using Bookstore.Application.Abstractions;
using Bookstore.Application.Authors.DTOs;
using Bookstore.Application.Authors.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Queries.GetAuthorById;

/// <summary>
/// Handles retrieval of a single author by identifier.
/// </summary>
/// <remarks>
/// Returns a not-found error if the author does not exist.
/// </remarks>
internal sealed class GetAuthorByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAuthorByIdQuery, Result<AuthorDto>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Looks up the author by their identifier and maps them to a DTO.
    /// </summary>
    /// <remarks>
    /// Returns a not-found error if the author does not exist.
    /// </remarks>
    /// <param name="query">The query containing the author identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the <see cref="AuthorDto"/>, or a <see cref="NotFoundError"/> if not found.</returns>
    public async ValueTask<Result<AuthorDto>> Handle(GetAuthorByIdQuery query, CancellationToken cancellationToken)
    {
        var author = await _context.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken);

        if (author is null)
            return Result.Failure<AuthorDto>(new NotFoundError("The author with the specified identifier was not found."));

        return Result.Success(author.ToDto());
    }
}
