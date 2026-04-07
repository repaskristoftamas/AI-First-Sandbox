using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Books.Events;

/// <summary>
/// Raised when an existing book's properties are updated.
/// </summary>
/// <param name="BookId">Identifier of the updated book.</param>
public sealed record BookUpdatedEvent(BookId BookId) : IDomainEvent;
