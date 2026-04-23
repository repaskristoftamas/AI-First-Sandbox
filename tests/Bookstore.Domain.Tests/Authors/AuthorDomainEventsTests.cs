using Bookstore.Domain.Authors;
using Bookstore.Domain.Authors.Events;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Authors;

public class AuthorDomainEventsTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_ShouldRaiseAuthorCreatedEvent()
    {
        // Act
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), _timeProvider).Value;

        // Assert
        author.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<AuthorCreatedEvent>()
            .AuthorId.ShouldBe(author.Id);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValidationFails()
    {
        // Act
        var result = Author.Create("", "Martin", new DateOnly(1952, 12, 5), _timeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Update_ShouldRaiseAuthorUpdatedEvent()
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1), _timeProvider).Value;
        author.ClearDomainEvents();

        // Act
        var result = author.Update("NewFirst", "NewLast", new DateOnly(1990, 6, 15), _timeProvider);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        author.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<AuthorUpdatedEvent>()
            .AuthorId.ShouldBe(author.Id);
    }

    [Fact]
    public void Update_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Arrange
        var author = Author.Create("OldFirst", "OldLast", new DateOnly(1980, 1, 1), _timeProvider).Value;
        author.ClearDomainEvents();

        // Act
        var result = author.Update("", "NewLast", new DateOnly(1990, 6, 15), _timeProvider);

        // Assert
        result.IsFailure.ShouldBeTrue();
        author.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Delete_ShouldRaiseAuthorDeletedEvent()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), _timeProvider).Value;
        author.ClearDomainEvents();

        // Act
        author.Delete(_timeProvider);

        // Assert
        author.IsDeleted.ShouldBeTrue();
        author.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<AuthorDeletedEvent>()
            .AuthorId.ShouldBe(author.Id);
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), _timeProvider).Value;

        // Act
        author.Delete(_timeProvider);

        // Assert
        author.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_ShouldSetDeletedAt()
    {
        // Arrange
        var author = Author.Create("Robert C.", "Martin", new DateOnly(1952, 12, 5), _timeProvider).Value;

        // Act
        author.Delete(_timeProvider);

        // Assert
        author.DeletedAt.ShouldBe(_timeProvider.GetUtcNow());
    }
}
