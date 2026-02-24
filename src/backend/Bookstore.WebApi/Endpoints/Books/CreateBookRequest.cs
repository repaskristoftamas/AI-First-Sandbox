namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for creating a new book in the catalog.
/// </summary>
public sealed record CreateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
