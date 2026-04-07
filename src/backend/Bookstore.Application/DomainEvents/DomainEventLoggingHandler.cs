using Bookstore.Application.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.DomainEvents;

/// <summary>
/// Logs all domain events for observability via structured logging.
/// </summary>
internal sealed class DomainEventLoggingHandler(
    ILogger<DomainEventLoggingHandler> logger) : INotificationHandler<DomainEventNotification>
{
    /// <summary>
    /// Handles a domain event notification by logging the event type name.
    /// </summary>
    /// <param name="notification">The domain event notification to handle.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A completed value task.</returns>
    public ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain event raised: {EventType}", notification.DomainEvent.GetType().Name);

        return ValueTask.CompletedTask;
    }
}
