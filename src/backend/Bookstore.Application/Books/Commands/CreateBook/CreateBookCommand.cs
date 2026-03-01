using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Books.Commands.CreateBook;

/// <summary>
/// Command to add a new book to the catalog, returning the generated identifier on success.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="AuthorId">Identifier of the author who wrote this book.</param>
/// <param name="ISBN">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
public sealed record CreateBookCommand(
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear) : ICommand<Result<Guid>>;
