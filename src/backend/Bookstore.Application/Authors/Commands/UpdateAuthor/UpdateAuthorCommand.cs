using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Commands.UpdateAuthor;

/// <summary>
/// Command to replace all editable properties of an existing author.
/// </summary>
/// <param name="Id">Identifier of the author to update.</param>
/// <param name="FirstName">First name of the author.</param>
/// <param name="LastName">Last name of the author.</param>
/// <param name="DateOfBirth">Date of birth of the author.</param>
public sealed record UpdateAuthorCommand(
    AuthorId Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth) : ICommand<Result>;
