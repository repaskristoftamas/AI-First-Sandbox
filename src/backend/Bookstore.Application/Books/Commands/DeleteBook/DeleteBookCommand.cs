using Bookstore.SharedKernel.Results;
using MediatR;

namespace Bookstore.Application.Books.Commands.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : IRequest<Result>;
