namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for creating a new book in the catalog.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="AuthorId">Identifier of the author who wrote this book.</param>
/// <param name="ISBN">ISBN-13 as 13 digits with no hyphens or spaces (978/979 prefix). Example: 9780132350884.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
public sealed record CreateBookRequest(
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear);
