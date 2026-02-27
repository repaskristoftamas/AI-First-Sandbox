using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Commands.CreateAuthor;

/// <summary>
/// Command to add a new author to the catalog, returning the generated identifier on success.
/// </summary>
/// <param name="FirstName">First name of the author.</param>
/// <param name="LastName">Last name of the author.</param>
/// <param name="DateOfBirth">Date of birth of the author.</param>
public sealed record CreateAuthorCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth) : ICommand<Result<Guid>>;
