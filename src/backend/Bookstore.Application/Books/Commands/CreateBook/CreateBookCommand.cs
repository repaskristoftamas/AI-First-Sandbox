using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.CreateBook;

/// <summary>
/// Command to add a new book to the catalog, returning the generated identifier on success.
/// </summary>
public sealed record CreateBookCommand(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : ICommand<Result<Guid>>;
