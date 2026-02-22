namespace Bookstore.WebApi.Endpoints.Books;

public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
