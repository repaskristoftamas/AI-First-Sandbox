namespace Bookstore.WebApi.Endpoints.Books;

public sealed record CreateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
