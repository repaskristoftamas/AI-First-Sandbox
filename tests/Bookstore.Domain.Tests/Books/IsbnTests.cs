using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Books;

public class IsbnTests
{
    [Theory]
    [InlineData("9780132350884")]
    [InlineData("9780134494166")]
    [InlineData("9780135957059")]
    [InlineData("9790000000001")]
    public void Create_ShouldReturnIsbn_WhenValueIsValid(string value)
    {
        // Act
        var result = Isbn.Create(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnIsbnRequired_WhenValueIsEmpty(string value)
    {
        // Act
        var result = Isbn.Create(value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldContain(f => f.Code == BookErrorCodes.IsbnRequired);
    }

    [Fact]
    public void Create_ShouldReturnIsbnRequired_WhenValueIsNull()
    {
        // Act
        var result = Isbn.Create(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldContain(f => f.Code == BookErrorCodes.IsbnRequired);
    }

    [Theory]
    [InlineData("978-0132350884", "hyphens")]
    [InlineData("1234567890123", "wrong prefix")]
    [InlineData("978012335088X", "contains letter")]
    [InlineData("978013235", "too short")]
    [InlineData("97801323508841", "too long")]
    [InlineData("HELLO", "not digits")]
    public void Create_ShouldReturnIsbnInvalidFormat_WhenFormatIsWrong(string value, string _)
    {
        // Act
        var result = Isbn.Create(value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldContain(f => f.Code == BookErrorCodes.IsbnInvalidFormat);
    }

    [Theory]
    [InlineData("9780000000000")]
    [InlineData("9780132350880")]
    [InlineData("9790000000000")]
    public void Create_ShouldReturnIsbnInvalidCheckDigit_WhenCheckDigitIsWrong(string value)
    {
        // Act
        var result = Isbn.Create(value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldContain(f => f.Code == BookErrorCodes.IsbnInvalidCheckDigit);
    }

    [Fact]
    public void ToString_ShouldReturnRawValue()
    {
        // Arrange
        var isbn = Isbn.Create("9780132350884").Value;

        // Act & Assert
        isbn.ToString().ShouldBe("9780132350884");
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenValuesAreEqual()
    {
        // Arrange
        var isbn1 = Isbn.Create("9780132350884").Value;
        var isbn2 = Isbn.Create("9780132350884").Value;

        // Act & Assert
        isbn1.ShouldBe(isbn2);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenValuesAreDifferent()
    {
        // Arrange
        var isbn1 = Isbn.Create("9780132350884").Value;
        var isbn2 = Isbn.Create("9780134494166").Value;

        // Act & Assert
        isbn1.ShouldNotBe(isbn2);
    }

    [Theory]
    [InlineData("978")]
    [InlineData("")]
    [InlineData("12345")]
    public void HasValidCheckDigit_ShortString_ShouldReturnFalse(string value)
    {
        // Act
        var result = Isbn.HasValidCheckDigit(value);

        // Assert
        result.ShouldBeFalse();
    }
}
