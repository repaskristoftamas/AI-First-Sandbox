using Bookstore.SharedKernel.Results;
using FluentValidation.Results;

namespace Bookstore.Application.Extensions;

/// <summary>
/// Extension methods for converting <see cref="ValidationResult"/> into the Result pattern.
/// </summary>
internal static class ValidationResultExtensions
{
    /// <summary>
    /// Converts a failed <see cref="ValidationResult"/> into a <see cref="Result"/> failure,
    /// joining all error messages with a semicolon separator.
    /// </summary>
    /// <param name="validationResult">The failed validation result to convert.</param>
    /// <returns>A failed <see cref="Result"/> containing a <see cref="ValidationError"/>.</returns>
    internal static Result ToFailureResult(this ValidationResult validationResult) =>
        Result.Failure(new ValidationError(string.Join("; ", validationResult.Errors.Select(f => f.ErrorMessage))));

    /// <summary>
    /// Converts a failed <see cref="ValidationResult"/> into a <see cref="Result{T}"/> failure,
    /// joining all error messages with a semicolon separator.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="validationResult">The failed validation result to convert.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing a <see cref="ValidationError"/>.</returns>
    internal static Result<T> ToFailureResult<T>(this ValidationResult validationResult) =>
        Result.Failure<T>(new ValidationError(string.Join("; ", validationResult.Errors.Select(f => f.ErrorMessage))));
}
