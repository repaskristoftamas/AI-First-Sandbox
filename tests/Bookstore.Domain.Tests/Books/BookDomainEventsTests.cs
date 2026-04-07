using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Books.Events;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Books;

public class BookDomainEventsTests
{
    [Fact]
    public void Create_ShouldRaiseBookCreatedEvent()
    {
        // Act
        var book = Book.Create("Clean Architecture", AuthorId.New(), Isbn.Create("9780134494166").Value, 39.99m, 2017, TimeProvider.System).Value;

        // Assert
        book.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<BookCreatedEvent>()
            .BookId.ShouldBe(book.Id);
    }

    [Fact]
    public void Create_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Act
        var result = Book.Create("", AuthorId.New(), Isbn.Create("9780134494166").Value, 39.99m, 2017, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        // No Book entity is created on failure, so no domain events can exist.
        // Accessing Value on a failed result throws, confirming no entity was instantiated.
        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Update_ShouldRaiseBookUpdatedEvent()
    {
        // Arrange
        var book = CreateTestBook();
        book.ClearDomainEvents();

        // Act
        var result = book.Update("Clean Code", AuthorId.New(), Isbn.Create("9780132350884").Value, 44.99m, 2008, TimeProvider.System);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        book.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<BookUpdatedEvent>()
            .BookId.ShouldBe(book.Id);
    }

    [Fact]
    public void Update_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Arrange
        var book = CreateTestBook();
        book.ClearDomainEvents();

        // Act
        var result = book.Update("", AuthorId.New(), Isbn.Create("9780134494166").Value, 39.99m, 2017, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
        book.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Delete_ShouldRaiseBookDeletedEvent()
    {
        // Arrange
        var book = CreateTestBook();
        book.ClearDomainEvents();

        // Act
        book.Delete();

        // Assert
        book.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<BookDeletedEvent>()
            .BookId.ShouldBe(book.Id);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var book = CreateTestBook();
        book.DomainEvents.ShouldNotBeEmpty();

        // Act
        book.ClearDomainEvents();

        // Assert
        book.DomainEvents.ShouldBeEmpty();
    }

    /// <summary>
    /// Creates a valid <see cref="Book"/> instance with default test values.
    /// </summary>
    private static Book CreateTestBook()
        => Book.Create("Clean Architecture", AuthorId.New(), Isbn.Create("9780134494166").Value, 39.99m, 2017, TimeProvider.System).Value;
}
