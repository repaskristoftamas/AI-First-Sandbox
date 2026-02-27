using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Commands.DeleteAuthor;

/// <summary>
/// Command to remove an existing author from the catalog by their identifier.
/// </summary>
/// <param name="Id">Identifier of the author to delete.</param>
public sealed record DeleteAuthorCommand(AuthorId Id) : ICommand<Result>;
