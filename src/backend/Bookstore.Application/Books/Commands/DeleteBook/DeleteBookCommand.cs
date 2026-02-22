using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.DeleteBook;

public sealed record DeleteBookCommand(BookId Id) : ICommand<Result>;
