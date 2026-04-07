using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Users.Events;

/// <summary>
/// Raised when an existing user's details are modified.
/// </summary>
/// <param name="UserId">Identifier of the updated user.</param>
public sealed record UserUpdatedEvent(UserId UserId) : IDomainEvent;
