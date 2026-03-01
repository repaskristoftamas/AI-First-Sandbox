namespace Bookstore.SharedKernel.Results;

/// <summary>
/// Represents a single field-level validation failure, carrying the property name,
/// a machine-readable code, and a human-readable description.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation.</param>
/// <param name="Code">Machine-readable error code for programmatic consumption.</param>
/// <param name="Description">Human-readable message describing the validation failure.</param>
public sealed record FieldValidationFailure(string PropertyName, string Code, string Description);
