using Bookstore.SharedKernel.Abstractions;
using Bookstore.SharedKernel.Results;

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
    /// <returns>A successful result containing the new <see cref="Author"/>, or a validation error.</returns>
    public static Result<Author> Create(string firstName, string lastName, DateOnly dateOfBirth)
    {
        var validation = Validate(firstName, lastName, dateOfBirth);
        if (validation.IsFailure)
            return Result.Failure<Author>(validation.Error);

        return Result.Success(new Author
        {
            Id = AuthorId.New(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        });
    }

    /// <summary>
    /// Replaces all mutable properties of the author with the provided values.
    /// </summary>
    /// <param name="firstName">First name of the author.</param>
    /// <param name="lastName">Last name of the author.</param>
    /// <param name="dateOfBirth">Date of birth of the author.</param>
    /// <returns>A success result, or a validation error if any value is invalid.</returns>
    public Result Update(string firstName, string lastName, DateOnly dateOfBirth)
    {
        var validation = Validate(firstName, lastName, dateOfBirth);
        if (validation.IsFailure)
            return validation;

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;

        return Result.Success();
    }

    /// <summary>
    /// Validates the author fields and returns a failure result if any value is invalid.
    /// Shared by <see cref="Create"/> and <see cref="Update"/> to eliminate duplication.
    /// </summary>
    private static Result Validate(string firstName, string lastName, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(FirstName), AuthorErrorCodes.FirstNameRequired, "First name is required.")]));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(LastName), AuthorErrorCodes.LastNameRequired, "Last name is required.")]));

        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(DateOfBirth), AuthorErrorCodes.DobInFuture, "Date of birth must be in the past.")]));

        return Result.Success();
    }
}
