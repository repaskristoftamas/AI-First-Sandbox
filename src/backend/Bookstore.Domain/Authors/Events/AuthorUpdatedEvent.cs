using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Authors.Events;

/// <summary>
/// Raised when an existing author's properties are updated.
/// </summary>
/// <param name="AuthorId">Identifier of the updated author.</param>
public sealed record AuthorUpdatedEvent(AuthorId AuthorId) : IDomainEvent;
