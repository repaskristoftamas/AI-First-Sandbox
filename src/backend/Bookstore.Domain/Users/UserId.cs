namespace Bookstore.Domain.Users;

/// <summary>
/// Strongly-typed identifier for the <see cref="User"/> entity.
/// </summary>
/// <param name="Value">Underlying <see cref="Guid"/> value of the identifier.</param>
public readonly record struct UserId(Guid Value)
{
    /// <summary>
    /// Generates a new unique user identifier.
    /// </summary>
    /// <returns>A new <see cref="UserId"/> wrapping a freshly generated <see cref="Guid"/>.</returns>
    public static UserId New() => new(Guid.NewGuid());
}
