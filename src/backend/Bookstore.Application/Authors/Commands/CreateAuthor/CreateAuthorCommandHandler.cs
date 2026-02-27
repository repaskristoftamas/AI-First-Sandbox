using Bookstore.Application.Abstractions;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Commands.CreateAuthor;

/// <summary>
/// Handles creation of a new author.
/// </summary>
internal sealed class CreateAuthorCommandHandler(IApplicationDbContext context) : ICommandHandler<CreateAuthorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Creates a new author and returns its identifier.
    /// </summary>
    /// <param name="command">The command containing the author details to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing the new author's identifier.</returns>
    public async ValueTask<Result<Guid>> Handle(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = Author.Create(command.FirstName, command.LastName, command.DateOfBirth);

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(author.Id.Value);
    }
}
