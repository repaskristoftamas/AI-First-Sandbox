using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Authors.Events;

/// <summary>
/// Raised when an author is removed from the catalog.
/// </summary>
/// <param name="AuthorId">Identifier of the deleted author.</param>
public sealed record AuthorDeletedEvent(AuthorId AuthorId) : IDomainEvent;
