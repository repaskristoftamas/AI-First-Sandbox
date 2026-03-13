using Mediator;

namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for domain events dispatched after successful persistence.
/// </summary>
/// <remarks>
/// Extends <see cref="INotification"/> to enable in-process dispatch via Mediator,
/// keeping the door open for out-of-process transport (e.g., MassTransit) later.
/// </remarks>
public interface IDomainEvent : INotification;
