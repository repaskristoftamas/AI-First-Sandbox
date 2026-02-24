using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.DeleteBook;

/// <summary>
/// Command to remove an existing book from the catalog by its identifier.
/// </summary>
/// <param name="Id">Identifier of the book to delete.</param>
public sealed record DeleteBookCommand(BookId Id) : ICommand<Result>;
