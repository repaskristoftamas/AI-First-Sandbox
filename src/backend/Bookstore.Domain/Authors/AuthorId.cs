namespace Bookstore.Domain.Authors;

/// <summary>
/// Strongly-typed identifier for the <see cref="Author"/> entity.
/// </summary>
/// <param name="Value">Underlying <see cref="Guid"/> value of the identifier.</param>
public readonly record struct AuthorId(Guid Value)
{
    /// <summary>
    /// Generates a new unique author identifier.
    /// </summary>
    /// <returns>A new <see cref="AuthorId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static AuthorId New() => new(Guid.NewGuid());
}
