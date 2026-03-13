namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Base class for all domain entities, providing a strongly-typed identifier and domain event support.
/// </summary>
public abstract class EntityBase<TId> : IHasDomainEvents where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Records a domain event to be dispatched after the entity is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();
}
