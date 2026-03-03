using System.Text.RegularExpressions;

namespace Bookstore.Application.Books;

/// <summary>
/// Shared ISBN validation utilities used by book command validators.
/// </summary>
public static partial class IsbnValidation
{
    /// <summary>
    /// Matches a valid ISBN-13: exactly 13 digits starting with 978 or 979.
    /// </summary>
    [GeneratedRegex(@"^(978|979)\d{10}$")]
    public static partial Regex Isbn13Regex();
}
