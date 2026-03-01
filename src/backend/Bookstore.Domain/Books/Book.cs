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
        var validation = Validate(title, author, isbn, price, publicationYear, timeProvider);
        if (validation.IsFailure)
            return Result.Failure<Book>(validation.Error);
    
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
        var validation = Validate(title, author, isbn, price, publicationYear, timeProvider);
        if (validation.IsFailure)
            return validation;

        Title = title;
        Author = author;
        ISBN = isbn;
        Price = price;
        PublicationYear = publicationYear;

        return Result.Success();
    }

    /// <summary>
    /// Validates the book fields and returns a failure result if any value is invalid.
    /// Shared by <see cref="Create"/> and <see cref="Update"/> to eliminate duplication.
    /// </summary>
    private static Result Validate(string title, string author, string isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Title), BookErrorCodes.TitleRequired, "Title is required.")]));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Author), BookErrorCodes.AuthorRequired, "Author is required.")]));

        if (string.IsNullOrWhiteSpace(isbn))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(ISBN), BookErrorCodes.IsbnRequired, "ISBN is required.")]));

        if (price <= 0)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Price), BookErrorCodes.PriceInvalid, "Price must be greater than zero.")]));

        if (publicationYear < 1450 || publicationYear > timeProvider.GetUtcNow().Year)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(PublicationYear), BookErrorCodes.PublicationYearInvalid, "Publication year must be a valid year.")]));

        return Result.Success();
    }
}
