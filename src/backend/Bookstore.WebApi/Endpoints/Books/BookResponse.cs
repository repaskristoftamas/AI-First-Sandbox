namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API response model representing a book returned to the client.
/// </summary>
public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
