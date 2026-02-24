namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Base class for all domain entities, providing a strongly-typed identifier.
/// </summary>
public abstract class EntityBase<TId> where TId : notnull
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;
}
