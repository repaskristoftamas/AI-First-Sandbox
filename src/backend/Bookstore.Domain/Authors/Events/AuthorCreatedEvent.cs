using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Authors.Events;

/// <summary>
/// Raised when a new author is created in the catalog.
/// </summary>
/// <param name="AuthorId">Identifier of the newly created author.</param>
public sealed record AuthorCreatedEvent(AuthorId AuthorId) : IDomainEvent;
