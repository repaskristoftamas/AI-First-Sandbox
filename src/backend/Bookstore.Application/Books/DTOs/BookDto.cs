namespace Bookstore.Application.Books.DTOs;

/// <summary>
/// Data transfer object representing a book returned from query operations.
/// </summary>
public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
