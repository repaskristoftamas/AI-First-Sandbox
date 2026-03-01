namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for updating an existing book's properties.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="AuthorId">Identifier of the author who wrote this book.</param>
/// <param name="ISBN">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
public sealed record UpdateBookRequest(
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear);
