namespace Bookstore.Domain.Books;

/// <summary>
/// Machine-readable error codes for Book domain operations.
/// </summary>
public static class BookErrorCodes
{
    /// <summary>The requested book does not exist.</summary>
    public const string NotFound = "BOOK_NOT_FOUND";

    /// <summary>A book with the given ISBN already exists.</summary>
    public const string IsbnConflict = "BOOK_ISBN_CONFLICT";

    /// <summary>The referenced author does not exist.</summary>
    public const string AuthorNotFound = "BOOK_AUTHOR_NOT_FOUND";

    /// <summary>Title is missing or whitespace.</summary>
    public const string TitleRequired = "BOOK_TITLE_REQUIRED";

    /// <summary>Title exceeds the maximum allowed length.</summary>
    public const string TitleTooLong = "BOOK_TITLE_TOO_LONG";

    /// <summary>Author identifier is missing or empty.</summary>
    public const string AuthorRequired = "BOOK_AUTHOR_REQUIRED";

    /// <summary>ISBN is missing or whitespace.</summary>
    public const string IsbnRequired = "BOOK_ISBN_REQUIRED";

    /// <summary>ISBN exceeds the maximum allowed length.</summary>
    public const string IsbnTooLong = "BOOK_ISBN_TOO_LONG";

    /// <summary>ISBN does not match the expected ISBN-13 format.</summary>
    public const string IsbnInvalidFormat = "BOOK_ISBN_INVALID_FORMAT";

    /// <summary>Price is not a positive number.</summary>
    public const string PriceInvalid = "BOOK_PRICE_INVALID";

    /// <summary>Publication year is not within the valid range.</summary>
    public const string PublicationYearInvalid = "BOOK_PUBLICATION_YEAR_INVALID";
}
