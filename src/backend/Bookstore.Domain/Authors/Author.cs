using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Authors;

/// <summary>
/// Domain entity representing an author in the bookstore catalog.
/// </summary>
public sealed class Author : AuditableEntity<AuthorId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Author"/> class.
    /// </summary>
    /// <remarks>
    /// Required by EF Core for materialization.
    /// </remarks>
    private Author() { }

    /// <summary>
    /// First name of the author.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Last name of the author.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Date of birth of the author.
    /// </summary>
    public DateOnly DateOfBirth { get; private set; }

    /// <summary>
    /// Factory method that creates a new author with a generated identifier.
    /// </summary>
    /// <param name="firstName">First name of the author.</param>
    /// <param name="lastName">Last name of the author.</param>
    /// <param name="dateOfBirth">Date of birth of the author.</param>
    /// <returns>A new <see cref="Author"/> instance with a unique identifier.</returns>
    public static Author Create(string firstName, string lastName, DateOnly dateOfBirth) =>
        new()
        {
            Id = AuthorId.New(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        };

    /// <summary>
    /// Replaces all mutable properties of the author with the provided values.
    /// </summary>
    /// <param name="firstName">First name of the author.</param>
    /// <param name="lastName">Last name of the author.</param>
    /// <param name="dateOfBirth">Date of birth of the author.</param>
    public void Update(string firstName, string lastName, DateOnly dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }
}
