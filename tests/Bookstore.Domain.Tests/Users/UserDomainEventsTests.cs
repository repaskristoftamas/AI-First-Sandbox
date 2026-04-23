using Bookstore.Domain.Users;
using Bookstore.Domain.Users.Events;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Users;

public class UserDomainEventsTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent()
    {
        // Act
        var user = User.Create("john@example.com", "hashed-password", [Role.User]).Value;

        // Assert
        user.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<UserCreatedEvent>()
            .UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValidationFails()
    {
        // Act
        var result = User.Create("", "hashed-password", [Role.User]);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Update_ShouldRaiseUserUpdatedEvent()
    {
        // Arrange
        var user = User.Create("old@example.com", "hashed", [Role.User]).Value;
        user.ClearDomainEvents();

        // Act
        var result = user.Update("new@example.com", [Role.Admin]);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<UserUpdatedEvent>()
            .UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void Update_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Arrange
        var user = User.Create("old@example.com", "hashed", [Role.User]).Value;
        user.ClearDomainEvents();

        // Act
        var result = user.Update("", [Role.User]);

        // Assert
        result.IsFailure.ShouldBeTrue();
        user.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void UpdatePasswordHash_ShouldRaiseUserUpdatedEvent()
    {
        // Arrange
        var user = User.Create("john@example.com", "old-hash", [Role.User]).Value;
        user.ClearDomainEvents();

        // Act
        var result = user.UpdatePasswordHash("new-hash");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<UserUpdatedEvent>()
            .UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void UpdatePasswordHash_ShouldNotRaiseEvent_WhenValidationFails()
    {
        // Arrange
        var user = User.Create("john@example.com", "old-hash", [Role.User]).Value;
        user.ClearDomainEvents();

        // Act
        var result = user.UpdatePasswordHash("");

        // Assert
        result.IsFailure.ShouldBeTrue();
        user.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Delete_ShouldRaiseUserDeletedEvent()
    {
        // Arrange
        var user = User.Create("john@example.com", "hashed", [Role.User]).Value;
        user.ClearDomainEvents();

        // Act
        user.Delete(_timeProvider);

        // Assert
        user.IsDeleted.ShouldBeTrue();
        user.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<UserDeletedEvent>()
            .UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = User.Create("john@example.com", "hashed", [Role.User]).Value;

        // Act
        user.Delete(_timeProvider);

        // Assert
        user.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_ShouldSetDeletedAt()
    {
        // Arrange
        var user = User.Create("john@example.com", "hashed", [Role.User]).Value;

        // Act
        user.Delete(_timeProvider);

        // Assert
        user.DeletedAt.ShouldBe(_timeProvider.GetUtcNow());
    }
}
