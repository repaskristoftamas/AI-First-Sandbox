using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Authors;

public class AuthorTests
{
    [Fact]
    public void Create_ShouldReturnAuthorWithCorrectProperties()
    {
        // Arrange
        const string firstName = "Robert C.";
        const string lastName = "Martin";
        var dateOfBirth = new DateOnly(1952, 12, 5);

        // Act
        var result = Author.Create(firstName, lastName, dateOfBirth);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.FirstName.ShouldBe(firstName);
        result.Value.LastName.ShouldBe(lastName);
        result.Value.DateOfBirth.ShouldBe(dateOfBirth);
    }

    [Theory]
    [InlineData("", "Martin", "First name is required.")]
    [InlineData("   ", "Martin", "First name is required.")]
    [InlineData("Robert", "", "Last name is required.")]
    [InlineData("Robert", "   ", "Last name is required.")]
    public void Create_ShouldReturnValidationError_WhenNameIsInvalid(
        string firstName, string lastName, string expectedMessage)
    {
        // Act
        var result = Author.Create(firstName, lastName, new DateOnly(1952, 12, 5));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenDateOfBirthIsInTheFuture()
    {
        // Act
        var result = Author.Create("Robert", "Martin", DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public void Update_ShouldModifyAuthorProperties()
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1)).Value;

        // Act
        var result = author.Update("NewFirst", "NewLast", new DateOnly(1990, 6, 15));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        author.FirstName.ShouldBe("NewFirst");
        author.LastName.ShouldBe("NewLast");
        author.DateOfBirth.ShouldBe(new DateOnly(1990, 6, 15));
    }

    [Theory]
    [InlineData("", "Last", "First name is required.")]
    [InlineData("First", "", "Last name is required.")]
    public void Update_ShouldReturnValidationError_WhenNameIsInvalid(
        string firstName, string lastName, string expectedMessage)
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1)).Value;

        // Act
        var result = author.Update(firstName, lastName, new DateOnly(1990, 6, 15));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }
}
