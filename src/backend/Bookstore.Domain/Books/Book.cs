using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Books;

/// <summary>
/// Domain entity representing a book in the bookstore catalog.
/// </summary>
public sealed class Book : AuditableEntity<BookId>
{
    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Book() { }

    /// <summary>
    /// Title of the book.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Name of the book's author.
    /// </summary>
    public string Author { get; private set; } = string.Empty;

    /// <summary>
    /// International Standard Book Number, uniquely identifying the publication.
    /// </summary>
    public string ISBN { get; private set; } = string.Empty;

    /// <summary>
    /// Retail price of the book.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Year the book was published.
    /// </summary>
    public int PublicationYear { get; private set; }

    /// <summary>
    /// Factory method that creates a new book with a generated identifier.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="author">Name of the book's author.</param>
    /// <param name="isbn">International Standard Book Number.</param>
    /// <param name="price">Retail price of the book.</param>
    /// <param name="publicationYear">Year the book was published.</param>
    /// <returns>A new <see cref="Book"/> instance with a unique identifier.</returns>
    public static Book Create(string title, string author, string isbn, decimal price, int publicationYear) =>
        new()
        {
            Id = BookId.New(),
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            PublicationYear = publicationYear
        };

    /// <summary>
    /// Replaces all mutable properties of the book with the provided values.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="author">Name of the book's author.</param>
    /// <param name="isbn">International Standard Book Number.</param>
    /// <param name="price">Retail price of the book.</param>
    /// <param name="publicationYear">Year the book was published.</param>
    public void Update(string title, string author, string isbn, decimal price, int publicationYear)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
        Price = price;
        PublicationYear = publicationYear;
    }
}
