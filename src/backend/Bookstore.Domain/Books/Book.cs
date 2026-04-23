using Bookstore.Domain.Authors;
using Bookstore.Domain.Books.Events;
using Bookstore.SharedKernel.Abstractions;
using Bookstore.SharedKernel.Results;

namespace Bookstore.Domain.Books;

/// <summary>
/// Domain entity representing a book in the bookstore catalog.
/// </summary>
public sealed class Book : AuditableEntity<BookId>, ISoftDeletable
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
    /// Identifier of the author who wrote this book.
    /// </summary>
    public AuthorId AuthorId { get; private set; }

    /// <summary>
    /// International Standard Book Number, uniquely identifying the publication.
    /// </summary>
    public Isbn ISBN { get; private set; }

    /// <summary>
    /// Retail price of the book.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Year the book was published.
    /// </summary>
    public int PublicationYear { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Factory method that creates a new book with a generated identifier.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="authorId">Identifier of the author who wrote this book.</param>
    /// <param name="isbn">Validated ISBN value object.</param>
    /// <param name="price">Retail price of the book.</param>
    /// <param name="publicationYear">Year the book was published.</param>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    /// <returns>
    /// A <see cref="Result{Book}"/> containing the new <see cref="Book"/> instance on success,
    /// or a <see cref="ValidationError"/> if any argument is invalid.
    /// </returns>
    public static Result<Book> Create(string title, AuthorId authorId, Isbn isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        var validation = Validate(title, isbn, price, publicationYear, timeProvider);
        if (validation.IsFailure)
            return Result.Failure<Book>(validation.Error);

        var book = new Book
        {
            Id = BookId.New(),
            Title = title,
            AuthorId = authorId,
            ISBN = isbn,
            Price = price,
            PublicationYear = publicationYear
        };

        book.AddDomainEvent(new BookCreatedEvent(book.Id));

        return Result.Success(book);
    }

    /// <summary>
    /// Replaces all mutable properties of the book with the provided values.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="authorId">Identifier of the author who wrote this book.</param>
    /// <param name="isbn">Validated ISBN value object.</param>
    /// <param name="price">Retail price of the book.</param>
    /// <param name="publicationYear">Year the book was published.</param>
    /// <param name="timeProvider">Provides the current date for publication year validation.</param>
    /// <returns>
    /// A success <see cref="Result"/> if all values are valid, or a <see cref="ValidationError"/> otherwise.
    /// </returns>
    public Result Update(string title, AuthorId authorId, Isbn isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        var validation = Validate(title, isbn, price, publicationYear, timeProvider);
        if (validation.IsFailure)
            return validation;

        Title = title;
        AuthorId = authorId;
        ISBN = isbn;
        Price = price;
        PublicationYear = publicationYear;

        AddDomainEvent(new BookUpdatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks this book as soft-deleted and raises the <see cref="BookDeletedEvent"/>.
    /// </summary>
    /// <param name="timeProvider">Provides the current time used to stamp <see cref="DeletedAt"/>.</param>
    public void Delete(TimeProvider timeProvider)
    {
        IsDeleted = true;
        DeletedAt = timeProvider.GetUtcNow();
        AddDomainEvent(new BookDeletedEvent(Id));
    }

    /// <summary>
    /// Last-resort invariant guard that protects structural integrity regardless of entry point.
    /// </summary>
    /// <remarks>
    /// Primary validation is handled by FluentValidation at the application boundary.
    /// Shared by <see cref="Create"/> and <see cref="Update"/> to eliminate duplication.
    /// </remarks>
    private static Result Validate(string title, Isbn isbn, decimal price, int publicationYear, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Title), BookErrorCodes.TitleRequired, "Title is required.")]));

        if (isbn == default)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(ISBN), BookErrorCodes.IsbnRequired, "ISBN is required.")]));

        if (price <= 0)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Price), BookErrorCodes.PriceInvalid, "Price must be greater than zero.")]));

        if (publicationYear < 1450 || publicationYear > timeProvider.GetUtcNow().Year)
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(PublicationYear), BookErrorCodes.PublicationYearInvalid, "Publication year must be a valid year.")]));

        return Result.Success();
    }
}
