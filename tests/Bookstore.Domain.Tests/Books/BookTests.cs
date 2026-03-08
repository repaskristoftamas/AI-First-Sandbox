using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Books;

public class BookTests
{
    private static readonly AuthorId TestAuthorId = AuthorId.New();

    [Fact]
    public void Create_ShouldReturnBookWithCorrectProperties()
    {
        // Arrange
        const string title = "Clean Architecture";
        var authorId = AuthorId.New();
        const string isbn = "9780134494166";
        const decimal price = 39.99m;
        const int publicationYear = 2017;

        // Act
        var book = Book.Create(title, authorId, isbn, price, publicationYear, TimeProvider.System).Value;

        // Assert
        book.Id.Value.ShouldNotBe(Guid.Empty);
        book.Title.ShouldBe(title);
        book.AuthorId.ShouldBe(authorId);
        book.ISBN.ShouldBe(isbn);
        book.Price.ShouldBe(price);
        book.PublicationYear.ShouldBe(publicationYear);
    }

    [Fact]
    public void Update_ShouldModifyBookProperties()
    {
        // Arrange
        var book = Book.Create("Old Title", TestAuthorId, "9780000000000", 10m, 2000, TimeProvider.System).Value;
        var newAuthorId = AuthorId.New();

        // Act
        var result = book.Update("New Title", newAuthorId, "9781111111111", 20m, 2023, TimeProvider.System);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        book.Title.ShouldBe("New Title");
        book.AuthorId.ShouldBe(newAuthorId);
        book.ISBN.ShouldBe("9781111111111");
        book.Price.ShouldBe(20m);
        book.PublicationYear.ShouldBe(2023);
    }

    [Theory]
    [InlineData("", "9780000000000", "Title is required.")]
    [InlineData("   ", "9780000000000", "Title is required.")]
    [InlineData("Title", "", "ISBN is required.")]
    [InlineData("Title", "   ", "ISBN is required.")]
    public void Create_ShouldReturnValidationError_WhenStringFieldIsInvalid(
        string title, string isbn, string expectedMessage)
    {
        // Act
        var result = Book.Create(title, TestAuthorId, isbn, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Create_ShouldReturnValidationError_WhenPriceIsNotPositive(decimal price)
    {
        // Act
        var result = Book.Create("Title", TestAuthorId, "9780000000000", price, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Price must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenPublicationYearIsBeforePrintingPress()
    {
        // Act
        var result = Book.Create("Title", TestAuthorId, "9780000000000", 10m, -1, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenPublicationYearIsInFuture()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var futureYear = 2025;

        // Act
        var result = Book.Create("Title", TestAuthorId, "9780000000000", 10m, futureYear, fakeTimeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }

    [Theory]
    [InlineData("", "9780000000000", "Title is required.")]
    [InlineData("   ", "9780000000000", "Title is required.")]
    [InlineData("Title", "", "ISBN is required.")]
    [InlineData("Title", "   ", "ISBN is required.")]
    public void Update_ShouldReturnValidationError_WhenStringFieldIsInvalid(
        string title, string isbn, string expectedMessage)
    {
        // Arrange
        var book = Book.Create("Old Title", TestAuthorId, "9780000000000", 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update(title, TestAuthorId, isbn, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Update_ShouldReturnValidationError_WhenPriceIsNotPositive(decimal price)
    {
        // Arrange
        var book = Book.Create("Title", TestAuthorId, "9780000000000", 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update("Title", TestAuthorId, "9780000000000", price, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Price must be greater than zero.");
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenPublicationYearIsBeforePrintingPress()
    {
        // Arrange
        var book = Book.Create("Title", TestAuthorId, "9780000000000", 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update("Title", TestAuthorId, "9780000000000", 10m, -1, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenPublicationYearIsInFuture()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var book = Book.Create("Title", TestAuthorId, "9780000000000", 10m, 2000, TimeProvider.System).Value;
        var futureYear = 2025;

        // Act
        var result = book.Update("Title", TestAuthorId, "9780000000000", 10m, futureYear, fakeTimeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }
}
