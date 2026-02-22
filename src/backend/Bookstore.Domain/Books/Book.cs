using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Books;

public sealed class Book : AuditableEntity<BookId>
{
    private Book() { }

    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string ISBN { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int PublicationYear { get; private set; }

    public static Book Create(string title, string author, string isbn, decimal price, int publicationYear)
    {
        return new Book
        {
            Id = BookId.New(),
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            PublicationYear = publicationYear
        };
    }

    public void Update(string title, string author, string isbn, decimal price, int publicationYear)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
        Price = price;
        PublicationYear = publicationYear;
    }
}
