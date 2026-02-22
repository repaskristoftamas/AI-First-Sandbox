using Bookstore.SharedKernel.Results;
using MediatR;

namespace Bookstore.Application.Books.Commands.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : IRequest<Result<Guid>>;
