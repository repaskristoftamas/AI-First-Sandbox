using Bookstore.SharedKernel.Abstractions;
using Mediator;

namespace Bookstore.Application.Abstractions;

/// <summary>
/// Wraps an <see cref="IDomainEvent"/> as a Mediator <see cref="INotification"/>,
/// bridging the messaging-agnostic domain layer to the Mediator infrastructure.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
