using System.Text.RegularExpressions;
using Bookstore.SharedKernel.Results;

namespace Bookstore.Domain.Books;

/// <summary>
/// Self-validating value object representing an ISBN-13.
/// Encapsulates format validation (978/979 prefix, 13 digits) and check-digit verification.
/// </summary>
public readonly partial record struct Isbn
{
    /// <summary>
    /// The raw 13-digit ISBN string.
    /// </summary>
    public string Value { get; }

    private Isbn(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Isbn"/> after validating format and check digit.
    /// </summary>
    /// <param name="value">The raw ISBN string to validate.</param>
    /// <returns>
    /// A <see cref="Result{Isbn}"/> containing the validated ISBN on success,
    /// or a <see cref="ValidationError"/> if the value is empty, malformed, or has an invalid check digit.
    /// </returns>
    public static Result<Isbn> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Isbn>(new ValidationError([new FieldValidationFailure(nameof(Value), BookErrorCodes.IsbnRequired, "ISBN is required.")]));

        if (!IsValidFormat(value))
            return Result.Failure<Isbn>(new ValidationError([new FieldValidationFailure(nameof(Value), BookErrorCodes.IsbnInvalidFormat, "ISBN must be a valid ISBN-13 format (13 digits starting with 978 or 979).")]));

        if (!HasValidCheckDigit(value))
            return Result.Failure<Isbn>(new ValidationError([new FieldValidationFailure(nameof(Value), BookErrorCodes.IsbnInvalidCheckDigit, "ISBN check digit is invalid.")]));

        return Result.Success(new Isbn(value));
    }

    /// <summary>
    /// Returns whether the given string matches the ISBN-13 structural format (978/979 prefix, 13 digits).
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns><c>true</c> if the format is valid; otherwise <c>false</c>.</returns>
    public static bool IsValidFormat(string value) => Isbn13Regex().IsMatch(value);

    /// <summary>
    /// Returns whether the given 13-digit string has a valid ISBN-13 check digit.
    /// </summary>
    /// <param name="value">A 13-digit string to verify.</param>
    /// <returns><c>true</c> if the check digit is correct; otherwise <c>false</c>.</returns>
    public static bool HasValidCheckDigit(string value)
    {
        if (value is not { Length: 13 })
            return false;

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = value[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        var checkDigit = (10 - sum % 10) % 10;
        return value[12] - '0' == checkDigit;
    }

    /// <summary>
    /// Reconstitutes an <see cref="Isbn"/> from a trusted persistence store without re-validation.
    /// </summary>
    /// <param name="value">The raw ISBN string from the database.</param>
    /// <returns>An <see cref="Isbn"/> instance wrapping the stored value.</returns>
    internal static Isbn FromDatabase(string value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value;

    [GeneratedRegex(@"^(978|979)\d{10}$")]
    private static partial Regex Isbn13Regex();
}
