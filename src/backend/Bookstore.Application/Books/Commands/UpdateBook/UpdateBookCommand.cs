using Bookstore.SharedKernel.Results;
using MediatR;

namespace Bookstore.Application.Books.Commands.UpdateBook;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : IRequest<Result>;
