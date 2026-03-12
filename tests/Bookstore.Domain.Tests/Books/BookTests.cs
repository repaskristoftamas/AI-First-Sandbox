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
    private static readonly Isbn TestIsbn = Isbn.Create("9780134494166").Value;

    [Fact]
    public void Create_ShouldReturnBookWithCorrectProperties()
    {
        // Arrange
        const string title = "Clean Architecture";
        var authorId = AuthorId.New();
        var isbn = Isbn.Create("9780134494166").Value;
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
        var book = Book.Create("Old Title", TestAuthorId, Isbn.Create("9780000000002").Value, 10m, 2000, TimeProvider.System).Value;
        var newAuthorId = AuthorId.New();
        var newIsbn = Isbn.Create("9781111111113").Value;

        // Act
        var result = book.Update("New Title", newAuthorId, newIsbn, 20m, 2023, TimeProvider.System);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        book.Title.ShouldBe("New Title");
        book.AuthorId.ShouldBe(newAuthorId);
        book.ISBN.ShouldBe(newIsbn);
        book.Price.ShouldBe(20m);
        book.PublicationYear.ShouldBe(2023);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnValidationError_WhenTitleIsInvalid(string title)
    {
        // Act
        var result = Book.Create(title, TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Title is required.");
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenIsbnIsDefault()
    {
        // Act
        var result = Book.Create("Title", TestAuthorId, default, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("ISBN is required.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Create_ShouldReturnValidationError_WhenPriceIsNotPositive(decimal price)
    {
        // Act
        var result = Book.Create("Title", TestAuthorId, TestIsbn, price, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Price must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenPublicationYearIsBeforePrintingPress()
    {
        // Act
        var result = Book.Create("Title", TestAuthorId, TestIsbn, 10m, -1, TimeProvider.System);

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
        var result = Book.Create("Title", TestAuthorId, TestIsbn, 10m, futureYear, fakeTimeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ShouldReturnValidationError_WhenTitleIsInvalid(string title)
    {
        // Arrange
        var book = Book.Create("Old Title", TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update(title, TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Title is required.");
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenIsbnIsDefault()
    {
        // Arrange
        var book = Book.Create("Title", TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update("Title", TestAuthorId, default, 10m, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("ISBN is required.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Update_ShouldReturnValidationError_WhenPriceIsNotPositive(decimal price)
    {
        // Arrange
        var book = Book.Create("Title", TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update("Title", TestAuthorId, TestIsbn, price, 2000, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Price must be greater than zero.");
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenPublicationYearIsBeforePrintingPress()
    {
        // Arrange
        var book = Book.Create("Title", TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System).Value;

        // Act
        var result = book.Update("Title", TestAuthorId, TestIsbn, 10m, -1, TimeProvider.System);

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
        var book = Book.Create("Title", TestAuthorId, TestIsbn, 10m, 2000, TimeProvider.System).Value;
        var futureYear = 2025;

        // Act
        var result = book.Update("Title", TestAuthorId, TestIsbn, 10m, futureYear, fakeTimeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("Publication year must be a valid year.");
    }
}
