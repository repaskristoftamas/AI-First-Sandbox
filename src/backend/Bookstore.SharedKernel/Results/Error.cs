namespace Bookstore.SharedKernel.Results;

/// <summary>
/// Base error type used within the Result pattern to represent a failure reason.
/// </summary>
public abstract record Error(string Description);

/// <summary>
/// Represents a failure caused by a requested resource not being found.
/// </summary>
public sealed record NotFoundError(string Description) : Error(Description);

/// <summary>
/// Represents a failure caused by a conflicting state (e.g., duplicate resource).
/// </summary>
public sealed record ConflictError(string Description) : Error(Description);

/// <summary>
/// Represents a failure caused by invalid input or business rule violation.
/// </summary>
public sealed record ValidationError(string Description) : Error(Description);
