using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Books.Events;

/// <summary>
/// Raised when a book is removed from the catalog.
/// </summary>
/// <param name="BookId">Identifier of the deleted book.</param>
public sealed record BookDeletedEvent(BookId BookId) : IDomainEvent;
