using Bookstore.Domain.Authors;
using FluentAssertions;
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
        var author = Author.Create(firstName, lastName, dateOfBirth);

        // Assert
        author.Id.Value.Should().NotBeEmpty();
        author.FirstName.Should().Be(firstName);
        author.LastName.Should().Be(lastName);
        author.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Fact]
    public void Update_ShouldModifyAuthorProperties()
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1));

        // Act
        author.Update("NewFirst", "NewLast", new DateOnly(1990, 6, 15));

        // Assert
        author.FirstName.Should().Be("NewFirst");
        author.LastName.Should().Be("NewLast");
        author.DateOfBirth.Should().Be(new DateOnly(1990, 6, 15));
    }
}
