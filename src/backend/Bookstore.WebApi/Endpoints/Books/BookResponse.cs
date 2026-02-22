namespace Bookstore.WebApi.Endpoints.Books;

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
