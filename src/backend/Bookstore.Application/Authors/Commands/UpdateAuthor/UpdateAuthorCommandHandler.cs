using Bookstore.Application.Abstractions;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Commands.UpdateAuthor;

/// <summary>
/// Handles updating an author's properties.
/// </summary>
/// <remarks>
/// Returns a not-found error if the author does not exist.
/// </remarks>
internal sealed class UpdateAuthorCommandHandler(IApplicationDbContext context) : ICommandHandler<UpdateAuthorCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Applies the updated properties to an existing author.
    /// </summary>
    /// <param name="command">The command containing the updated author properties.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a <see cref="NotFoundError"/> if the author does not exist.</returns>
    public async ValueTask<Result> Handle(UpdateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (author is null)
            return Result.Failure(new NotFoundError("The author with the specified identifier was not found."));

        author.Update(command.FirstName, command.LastName, command.DateOfBirth);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
