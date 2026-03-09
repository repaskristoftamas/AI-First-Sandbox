using Bookstore.Application.Abstractions;
using Bookstore.Application.Extensions;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Authors.Commands.UpdateAuthor;

/// <summary>
/// Handles updating an author's properties.
/// </summary>
/// <remarks>
/// Returns a not-found error if the author does not exist.
/// </remarks>
internal sealed class UpdateAuthorCommandHandler(
    IApplicationDbContext context,
    IValidator<UpdateAuthorCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<UpdateAuthorCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IValidator<UpdateAuthorCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Applies the updated properties to an existing author.
    /// </summary>
    /// <param name="command">The command containing the updated author properties.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a <see cref="NotFoundError"/> if the author does not exist.</returns>
    public async ValueTask<Result> Handle(UpdateAuthorCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailureResult();

        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);

        if (author is null)
            return Result.Failure(new NotFoundError(AuthorErrorCodes.NotFound, "The author with the specified identifier was not found."));

        var updateResult = author.Update(command.FirstName, command.LastName, command.DateOfBirth, _timeProvider);

        if (updateResult.IsFailure)
            return updateResult;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
