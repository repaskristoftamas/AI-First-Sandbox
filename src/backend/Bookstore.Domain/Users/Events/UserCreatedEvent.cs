using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Users.Events;

/// <summary>
/// Raised when a new user is created in the system.
/// </summary>
/// <param name="UserId">Identifier of the newly created user.</param>
public sealed record UserCreatedEvent(UserId UserId) : IDomainEvent;
