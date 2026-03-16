namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API response model representing a book returned to the client.
/// </summary>
/// <param name="Id" example="b2c3d4e5-f6a7-8901-bcde-f12345678901">Unique identifier of the book.</param>
/// <param name="Title" example="To Kill a Mockingbird">Title of the book.</param>
/// <param name="AuthorId" example="a1b2c3d4-e5f6-7890-abcd-ef1234567890">Identifier of the author who wrote this book.</param>
/// <param name="ISBN" example="9780061120084">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price" example="19.99">Retail price of the book.</param>
/// <param name="PublicationYear" example="1960">Year the book was published.</param>
/// <param name="CreatedAt" example="2024-01-15T10:30:00+00:00">Timestamp when the book was added to the catalog.</param>
/// <param name="UpdatedAt" example="2024-06-20T14:45:00+00:00">Timestamp of the last update, or <c>null</c> if never updated.</param>
public sealed record BookResponse(
    Guid Id,
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
