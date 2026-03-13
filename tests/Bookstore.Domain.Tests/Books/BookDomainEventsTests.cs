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
        var book = Book.Create("Clean Architecture", AuthorId.New(), "9780134494166", 39.99m, 2017, TimeProvider.System).Value;

        // Assert
        book.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<BookCreatedEvent>()
            .BookId.ShouldBe(book.Id);
    }

    [Fact]
    public void Create_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Act
        var result = Book.Create("", AuthorId.New(), "9780134494166", 39.99m, 2017, TimeProvider.System);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var book = Book.Create("Clean Architecture", AuthorId.New(), "9780134494166", 39.99m, 2017, TimeProvider.System).Value;
        book.DomainEvents.ShouldNotBeEmpty();

        // Act
        book.ClearDomainEvents();

        // Assert
        book.DomainEvents.ShouldBeEmpty();
    }
}
