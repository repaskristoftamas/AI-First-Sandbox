using Bookstore.SharedKernel.Abstractions;

namespace Bookstore.Domain.Books.Events;

/// <summary>
/// Raised when a new book is created in the catalog.
/// </summary>
/// <param name="BookId">Identifier of the newly created book.</param>
public sealed record BookCreatedEvent(BookId BookId) : IDomainEvent;
