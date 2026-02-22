using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.UpdateBook;

public sealed record UpdateBookCommand(
    BookId Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : ICommand<Result>;
