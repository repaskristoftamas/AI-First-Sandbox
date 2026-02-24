using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.UpdateBook;

/// <summary>
/// Command to replace all editable properties of an existing book.
/// </summary>
/// <param name="Id">Identifier of the book to update.</param>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Name of the book's author.</param>
/// <param name="ISBN">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
public sealed record UpdateBookCommand(
    BookId Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear) : ICommand<Result>;
