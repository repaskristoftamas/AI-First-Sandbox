namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API response model representing a book returned to the client.
/// </summary>
/// <param name="Id">Unique identifier of the book.</param>
/// <param name="Title">Title of the book.</param>
/// <param name="AuthorId">Identifier of the author who wrote this book.</param>
/// <param name="ISBN">International Standard Book Number, uniquely identifying the publication.</param>
/// <param name="Price">Retail price of the book.</param>
/// <param name="PublicationYear">Year the book was published.</param>
/// <param name="CreatedAt">Timestamp when the book was added to the catalog.</param>
/// <param name="UpdatedAt">Timestamp of the last update, or <c>null</c> if never updated.</param>
public sealed record BookResponse(
    Guid Id,
    string Title,
    Guid AuthorId,
    string ISBN,
    decimal Price,
    int PublicationYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
