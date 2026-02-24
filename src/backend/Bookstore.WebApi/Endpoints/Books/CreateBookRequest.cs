namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for creating a new book in the catalog.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Name of the book's author.</param>
/// <param name="ISBN">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
public sealed record CreateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
