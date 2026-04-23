using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Users.Events;

/// <summary>
/// Raised when a user is deleted from the system.
/// </summary>
/// <param name="UserId">Identifier of the deleted user.</param>
public sealed record UserDeletedEvent(UserId UserId) : IDomainEvent;
