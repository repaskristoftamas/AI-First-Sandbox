using Bookstore.SharedKernel.Results;

namespace Bookstore.Domain.Books;

public static class BookErrors
{
    public static readonly Error NotFound = new("Book.NotFound", "The book with the specified identifier was not found.");
    public static Error Conflict(string isbn) => new("Book.Conflict", $"A book with ISBN '{isbn}' already exists.");
}
