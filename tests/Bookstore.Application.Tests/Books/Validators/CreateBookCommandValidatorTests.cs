using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Domain.Books;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Bookstore.Application.Tests.Books.Validators;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new(TimeProvider.System);

    [Theory]
    [InlineData("9780132350884")]
    [InlineData("9790000000000")]
    public void Validate_ShouldPass_WhenIsbnIsValidIsbn13(string isbn)
    {
        // Arrange
        var command = CreateCommand(isbn: isbn);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBN);
    }

    [Theory]
    [InlineData("978-0132350884", "hyphens")]
    [InlineData("1234567890123", "wrong prefix")]
    [InlineData("978012335088X", "contains letter")]
    [InlineData("978013235", "too short")]
    [InlineData("97801323508841", "too long")]
    [InlineData("HELLO", "not digits")]
    public void Validate_ShouldFail_WhenIsbnFormatIsInvalid(string isbn, string _)
    {
        // Arrange
        var command = CreateCommand(isbn: isbn);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
            .WithErrorCode(BookErrorCodes.IsbnInvalidFormat);
    }

    [Fact]
    public void Validate_ShouldFail_WhenIsbnExceedsMaxLength()
    {
        // Arrange
        var command = CreateCommand(isbn: "97801323508841");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
            .WithErrorCode(BookErrorCodes.IsbnTooLong);
    }

    /// <summary>
    /// Creates a valid command with optional overrides for targeted validation tests.
    /// </summary>
    private static CreateBookCommand CreateCommand(string isbn) =>
        new("Valid Title", Guid.NewGuid(), isbn, 10m, 2000);
}
