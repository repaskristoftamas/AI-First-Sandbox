using Xunit;
using Bookstore.Domain.Books;
using FluentAssertions;

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
        var book = Book.Create(title, author, isbn, price, publicationYear);

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
        var book = Book.Create("Old Title", "Old Author", "978-0000000000", 10m, 2000);

        // Act
        book.Update("New Title", "New Author", "978-1111111111", 20m, 2023);

        // Assert
        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.ISBN.Should().Be("978-1111111111");
        book.Price.Should().Be(20m);
        book.PublicationYear.Should().Be(2023);
    }
}
