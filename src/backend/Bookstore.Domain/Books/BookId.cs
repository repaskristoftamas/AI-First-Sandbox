namespace Bookstore.Domain.Books;

public readonly record struct BookId(Guid Value)
{
    public static BookId New() => new(Guid.NewGuid());
}
