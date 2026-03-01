namespace Bookstore.SharedKernel.Results;

/// <summary>
/// Base error type used within the Result pattern to represent a failure reason.
/// </summary>
/// <param name="Code">Machine-readable error code for programmatic consumption.</param>
/// <param name="Description">Human-readable message describing the failure.</param>
public abstract record Error(string Code, string Description);

/// <summary>
/// Represents a failure caused by a requested resource not being found.
/// </summary>
/// <param name="Code">Machine-readable error code for programmatic consumption.</param>
/// <param name="Description">Human-readable message describing what was not found.</param>
public sealed record NotFoundError(string Code, string Description) : Error(Code, Description);

/// <summary>
/// Represents a failure caused by a conflicting state (e.g., duplicate resource).
/// </summary>
/// <param name="Code">Machine-readable error code for programmatic consumption.</param>
/// <param name="Description">Human-readable message describing the conflict.</param>
public sealed record ConflictError(string Code, string Description) : Error(Code, Description);

/// <summary>
/// Represents a failure caused by invalid input or business rule violations.
/// Carries per-field failure details so API clients can act on each error programmatically.
/// </summary>
/// <param name="Failures">The collection of field-level validation failures.</param>
public sealed record ValidationError(IReadOnlyList<FieldValidationFailure> Failures)
    : Error("VALIDATION_FAILED", string.Join("; ", Failures.Select(f => f.Description)));
