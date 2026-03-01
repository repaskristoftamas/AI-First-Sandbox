using Bookstore.SharedKernel.Abstractions;
using Bookstore.SharedKernel.Results;

namespace Bookstore.Domain.Books;

/// <summary>
/// Domain entity representing a book in the bookstore catalog.
/// </summary>
public sealed class Book : AuditableEntity<BookId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Book"/> class.
    /// </summary>
    /// <remarks>
    /// Required by EF Core for materialization.
    /// </remarks>
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
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    /// <returns>
    /// A <see cref="Result{Book}"/> containing the new <see cref="Book"/> instance on success,
    /// or a <see cref="ValidationError"/> if any argument is invalid.
    /// </returns>
    public static Result<Book> Create(string title, string author, string isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Book>(new ValidationError("BOOK_TITLE_REQUIRED", "Title is required."));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure<Book>(new ValidationError("BOOK_AUTHOR_REQUIRED", "Author is required."));

        if (string.IsNullOrWhiteSpace(isbn))
            return Result.Failure<Book>(new ValidationError("BOOK_ISBN_REQUIRED", "ISBN is required."));

        if (price <= 0)
            return Result.Failure<Book>(new ValidationError("BOOK_PRICE_INVALID", "Price must be greater than zero."));

        if (publicationYear < 1450 || publicationYear > timeProvider.GetLocalNow().Year)
            return Result.Failure<Book>(new ValidationError("BOOK_PUBLICATION_YEAR_INVALID", "Publication year must be a valid year."));
    
        return Result.Success(new Book
        {
            Id = BookId.New(),
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            PublicationYear = publicationYear
        });
    }

    /// <summary>
    /// Replaces all mutable properties of the book with the provided values.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="author">Name of the book's author.</param>
    /// <param name="isbn">International Standard Book Number.</param>
    /// <param name="price">Retail price of the book.</param>
    /// <param name="publicationYear">Year the book was published.</param>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    /// <returns>
    /// A success <see cref="Result"/> if all values are valid, or a <see cref="ValidationError"/> otherwise.
    /// </returns>
    public Result Update(string title, string author, string isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new ValidationError("BOOK_TITLE_REQUIRED", "Title is required."));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure(new ValidationError("BOOK_AUTHOR_REQUIRED", "Author is required."));

        if (string.IsNullOrWhiteSpace(isbn))
            return Result.Failure(new ValidationError("BOOK_ISBN_REQUIRED", "ISBN is required."));

        if (price <= 0)
            return Result.Failure(new ValidationError("BOOK_PRICE_INVALID", "Price must be greater than zero."));

        if (publicationYear < 1450 || publicationYear > timeProvider.GetLocalNow().Year)
            return Result.Failure(new ValidationError("BOOK_PUBLICATION_YEAR_INVALID", "Publication year must be a valid year."));

        Title = title;
        Author = author;
        ISBN = isbn;
        Price = price;
        PublicationYear = publicationYear;

        return Result.Success();
    }
}
