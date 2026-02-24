namespace Bookstore.SharedKernel.Results;

/// <summary>
/// Base error type used within the Result pattern to represent a failure reason.
/// </summary>
/// <param name="Description">Human-readable message describing the failure.</param>
public abstract record Error(string Description);

/// <summary>
/// Represents a failure caused by a requested resource not being found.
/// </summary>
/// <param name="Description">Human-readable message describing what was not found.</param>
public sealed record NotFoundError(string Description) : Error(Description);

/// <summary>
/// Represents a failure caused by a conflicting state (e.g., duplicate resource).
/// </summary>
/// <param name="Description">Human-readable message describing the conflict.</param>
public sealed record ConflictError(string Description) : Error(Description);

/// <summary>
/// Represents a failure caused by invalid input or business rule violation.
/// </summary>
/// <param name="Description">Human-readable message describing the validation failure.</param>
public sealed record ValidationError(string Description) : Error(Description);
