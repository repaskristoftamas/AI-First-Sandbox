using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : ICommand<Result<Guid>>;
