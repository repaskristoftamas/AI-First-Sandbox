using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using FluentAssertions;
using Xunit;

namespace Bookstore.Domain.Tests.Books;

public class BookTests
{
    [Fact]
    public void Create_ShouldReturnBookWithCorrectProperties()
    {
        // Arrange
        const string title = "Clean Architecture";
        const string author = "Robert C. Martin";
        const string isbn = "978-0134494166";
        const decimal price = 39.99m;
        const int publicationYear = 2017;

        // Act
        var book = Book.Create(title, author, isbn, price, publicationYear).Value;

        // Assert
        book.Id.Value.Should().NotBeEmpty();
        book.Title.Should().Be(title);
        book.Author.Should().Be(author);
        book.ISBN.Should().Be(isbn);
        book.Price.Should().Be(price);
        book.PublicationYear.Should().Be(publicationYear);
    }

    [Fact]
    public void Update_ShouldModifyBookProperties()
    {
        // Arrange
        var book = Book.Create("Old Title", "Old Author", "978-0000000000", 10m, 2000).Value;

        // Act
        var result = book.Update("New Title", "New Author", "978-1111111111", 20m, 2023);

        // Assert
        result.IsSuccess.Should().BeTrue();
        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.ISBN.Should().Be("978-1111111111");
        book.Price.Should().Be(20m);
        book.PublicationYear.Should().Be(2023);
    }

    [Theory]
    [InlineData("", "Author", "978-0000000000", "Title is required.")]
    [InlineData("   ", "Author", "978-0000000000", "Title is required.")]
    [InlineData("Title", "", "978-0000000000", "Author is required.")]
    [InlineData("Title", "   ", "978-0000000000", "Author is required.")]
    [InlineData("Title", "Author", "", "ISBN is required.")]
    [InlineData("Title", "Author", "   ", "ISBN is required.")]
    public void Update_ShouldReturnValidationError_WhenStringFieldIsInvalid(
        string title, string author, string isbn, string expectedMessage)
    {
        // Arrange
        var book = Book.Create("Old Title", "Old Author", "978-0000000000", 10m, 2000).Value;

        // Act
        var result = book.Update(title, author, isbn, 10m, 2000);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Description.Should().Be(expectedMessage);
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenPriceIsNegative()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "978-0000000000", 10m, 2000).Value;

        // Act
        var result = book.Update("Title", "Author", "978-0000000000", -1m, 2000);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Description.Should().Be("Price cannot be negative.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9999)]
    public void Update_ShouldReturnValidationError_WhenPublicationYearIsInvalid(int publicationYear)
    {
        // Arrange
        var book = Book.Create("Title", "Author", "978-0000000000", 10m, 2000).Value;

        // Act
        var result = book.Update("Title", "Author", "978-0000000000", 10m, publicationYear);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Description.Should().Be("Publication year must be a valid year.");
    }
}
