namespace Bookstore.Application.Books.DTOs;

public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
