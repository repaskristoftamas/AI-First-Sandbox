using Bookstore.Application.Abstractions;
using Bookstore.Application.DomainEvents;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Authors.Events;
using Bookstore.Domain.Books;
using Bookstore.Domain.Books.Events;
using Bookstore.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.DomainEvents;

public class DomainEventLoggingHandlerTests
{
    private readonly Mock<ILogger<DomainEventLoggingHandler>> _loggerMock = new();
    private readonly DomainEventLoggingHandler _handler;

    public DomainEventLoggingHandlerTests()
    {
        _handler = new DomainEventLoggingHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AuthorCreatedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new AuthorCreatedEvent(AuthorId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_AuthorUpdatedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new AuthorUpdatedEvent(AuthorId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_AuthorDeletedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new AuthorDeletedEvent(AuthorId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_BookCreatedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new BookCreatedEvent(BookId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_BookUpdatedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new BookUpdatedEvent(BookId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_BookDeletedEvent_LogsInformation()
    {
        // Arrange
        var notification = new DomainEventNotification(new BookDeletedEvent(BookId.New()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_UnknownDomainEvent_LogsWarning()
    {
        // Arrange
        var notification = new DomainEventNotification(new UnknownTestEvent());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Warning);
    }

    /// <summary>
    /// Verifies that the logger was called exactly once at the specified log level.
    /// </summary>
    private void VerifyLogWasCalled(LogLevel level)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed record UnknownTestEvent : IDomainEvent;
}
