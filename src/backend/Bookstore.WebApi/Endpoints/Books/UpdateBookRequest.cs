namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// API request model for updating an existing book's properties.
/// </summary>
public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
