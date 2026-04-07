using Bookstore.Application.Abstractions;
using Bookstore.Application.DomainEvents;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Authors.Events;
using Bookstore.Domain.Books;
using Bookstore.Domain.Books.Events;
using Bookstore.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
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

    public static TheoryData<IDomainEvent, string> DomainEvents => new()
    {
        { new AuthorCreatedEvent(AuthorId.New()), nameof(AuthorCreatedEvent) },
        { new AuthorUpdatedEvent(AuthorId.New()), nameof(AuthorUpdatedEvent) },
        { new AuthorDeletedEvent(AuthorId.New()), nameof(AuthorDeletedEvent) },
        { new BookCreatedEvent(BookId.New()), nameof(BookCreatedEvent) },
        { new BookUpdatedEvent(BookId.New()), nameof(BookUpdatedEvent) },
        { new BookDeletedEvent(BookId.New()), nameof(BookDeletedEvent) },
    };

    [Theory]
    [MemberData(nameof(DomainEvents))]
    public async Task Handle_DomainEvent_LogsInformationWithEventType(IDomainEvent domainEvent, string expectedEventType)
    {
        // Arrange
        var notification = new DomainEventNotification(domainEvent);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information, expectedEventType);
    }

    [Fact]
    public async Task Handle_UnknownDomainEvent_LogsInformationWithEventType()
    {
        // Arrange
        var notification = new DomainEventNotification(new UnknownTestEvent());

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        VerifyLogWasCalled(LogLevel.Information, nameof(UnknownTestEvent));
    }

    /// <summary>
    /// Verifies that the logger was called exactly once at the specified log level
    /// with a message containing the expected event type name.
    /// </summary>
    private void VerifyLogWasCalled(LogLevel level, string expectedEventType)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(expectedEventType)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed record UnknownTestEvent : IDomainEvent;
}
