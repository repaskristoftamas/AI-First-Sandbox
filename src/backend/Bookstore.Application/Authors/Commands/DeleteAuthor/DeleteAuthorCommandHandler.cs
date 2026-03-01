using Bookstore.Application.Abstractions;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Commands.DeleteAuthor;

/// <summary>
/// Handles deletion of an author.
/// </summary>
/// <remarks>
/// Returns a not-found error if the author does not exist.
/// </remarks>
internal sealed class DeleteAuthorCommandHandler(IApplicationDbContext context) : ICommandHandler<DeleteAuthorCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Locates the author by identifier and removes them from the catalog.
    /// </summary>
    /// <param name="command">The command containing the identifier of the author to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a <see cref="NotFoundError"/> if the author does not exist.</returns>
    public async ValueTask<Result> Handle(DeleteAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (author is null)
            return Result.Failure(new NotFoundError(AuthorErrorCodes.NotFound, "The author with the specified identifier was not found."));

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
