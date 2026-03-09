namespace Bookstore.Domain.Authors;

/// <summary>
/// Machine-readable error codes for Author domain operations.
/// </summary>
public static class AuthorErrorCodes
{
    /// <summary>The requested author does not exist.</summary>
    public const string NotFound = "AUTHOR_NOT_FOUND";

    /// <summary>First name is missing or whitespace.</summary>
    public const string FirstNameRequired = "AUTHOR_FIRST_NAME_REQUIRED";

    /// <summary>First name exceeds the maximum allowed length.</summary>
    public const string FirstNameTooLong = "AUTHOR_FIRST_NAME_TOO_LONG";

    /// <summary>Last name is missing or whitespace.</summary>
    public const string LastNameRequired = "AUTHOR_LAST_NAME_REQUIRED";

    /// <summary>Last name exceeds the maximum allowed length.</summary>
    public const string LastNameTooLong = "AUTHOR_LAST_NAME_TOO_LONG";

    /// <summary>Date of birth is today or in the future.</summary>
    public const string DobInFuture = "AUTHOR_DOB_IN_FUTURE";

    /// <summary>The author still has books associated with them.</summary>
    public const string HasAssociatedBooks = "AUTHOR_HAS_ASSOCIATED_BOOKS";
}
