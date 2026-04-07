using Bookstore.Application.Abstractions;
using Bookstore.Domain.Authors.Events;
using Bookstore.Domain.Books.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.DomainEvents;

/// <summary>
/// Logs all domain events for observability via structured logging.
/// </summary>
internal sealed class DomainEventLoggingHandler(
    ILogger<DomainEventLoggingHandler> logger) : INotificationHandler<DomainEventNotification>
{
    private readonly ILogger<DomainEventLoggingHandler> _logger = logger;

    /// <summary>
    /// Handles a domain event notification by logging the event details.
    /// </summary>
    /// <param name="notification">The domain event notification to handle.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A completed value task.</returns>
    public ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        switch (notification.DomainEvent)
        {
            case AuthorCreatedEvent e:
                _logger.LogInformation("Author created: {AuthorId}", e.AuthorId.Value);
                break;
            case AuthorUpdatedEvent e:
                _logger.LogInformation("Author updated: {AuthorId}", e.AuthorId.Value);
                break;
            case AuthorDeletedEvent e:
                _logger.LogInformation("Author deleted: {AuthorId}", e.AuthorId.Value);
                break;
            case BookCreatedEvent e:
                _logger.LogInformation("Book created: {BookId}", e.BookId.Value);
                break;
            case BookUpdatedEvent e:
                _logger.LogInformation("Book updated: {BookId}", e.BookId.Value);
                break;
            case BookDeletedEvent e:
                _logger.LogInformation("Book deleted: {BookId}", e.BookId.Value);
                break;
            default:
                _logger.LogWarning("Unhandled domain event: {EventType}", notification.DomainEvent.GetType().Name);
                break;
        }

        return ValueTask.CompletedTask;
    }
}
