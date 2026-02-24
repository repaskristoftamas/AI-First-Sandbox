namespace Bookstore.Domain.Books;

/// <summary>
/// Strongly-typed identifier for the <see cref="Book"/> entity.
/// </summary>
public readonly record struct BookId(Guid Value)
{
    /// <summary>
    /// Generates a new unique book identifier.
    /// </summary>
    /// <returns>A new <see cref="BookId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static BookId New() => new(Guid.NewGuid());
}
