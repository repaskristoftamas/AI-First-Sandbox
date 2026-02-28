using Bookstore.Application.Abstractions;
using Bookstore.Application.Extensions;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using FluentValidation;
using Mediator;

namespace Bookstore.Application.Authors.Commands.CreateAuthor;

/// <summary>
/// Handles creation of a new author.
/// </summary>
internal sealed class CreateAuthorCommandHandler(
    IApplicationDbContext context,
    IValidator<CreateAuthorCommand> validator) : ICommandHandler<CreateAuthorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IValidator<CreateAuthorCommand> _validator = validator;

    /// <summary>
    /// Creates a new author and returns its identifier.
    /// </summary>
    /// <param name="command">The command containing the author details to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the new author's identifier.</returns>
    public async ValueTask<Result<Guid>> Handle(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailureResult<Guid>();

        //TODO when there are more properties, switch to parameter object
        var createResult = Author.Create(command.FirstName, command.LastName, command.DateOfBirth);

        if (createResult.IsFailure)
            return Result.Failure<Guid>(createResult.Error);

        var author = createResult.Value;
        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(author.Id.Value);
    }
}
