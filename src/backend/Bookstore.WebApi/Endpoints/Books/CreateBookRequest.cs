namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for creating a new book in the catalog.
/// </summary>
/// <param name="Title" example="To Kill a Mockingbird">Title of the book.</param>
/// <param name="AuthorId" example="a1b2c3d4-e5f6-7890-abcd-ef1234567890">Identifier of the author who wrote this book.</param>
/// <param name="ISBN" example="9780316769488">ISBN-13 as 13 digits with no hyphens or spaces (978/979 prefix).</param>
/// <param name="Price" example="19.99">Retail price of the book.</param>
/// <param name="PublicationYear" example="1960">Year the book was published.</param>
public sealed record CreateBookRequest(
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear);
