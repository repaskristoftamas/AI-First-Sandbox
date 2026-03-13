namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Contract for entities that collect domain events for deferred dispatch.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Domain events raised by this entity, pending dispatch.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Removes all pending domain events from the collection.
    /// </summary>
    void ClearDomainEvents();
}
