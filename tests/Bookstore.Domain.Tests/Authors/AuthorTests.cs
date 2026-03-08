using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using FluentAssertions; //TODO: switch to Shouldly for consistency with other tests
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
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Value.Should().NotBeEmpty();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.DateOfBirth.Should().Be(dateOfBirth);
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
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Description.Should().Be(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenDateOfBirthIsInTheFuture()
    {
        // Act
        var result = Author.Create("Robert", "Martin", DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public void Update_ShouldModifyAuthorProperties()
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1)).Value;

        // Act
        var result = author.Update("NewFirst", "NewLast", new DateOnly(1990, 6, 15));

        // Assert
        result.IsSuccess.Should().BeTrue();
        author.FirstName.Should().Be("NewFirst");
        author.LastName.Should().Be("NewLast");
        author.DateOfBirth.Should().Be(new DateOnly(1990, 6, 15));
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
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Description.Should().Be(expectedMessage);
    }
}
