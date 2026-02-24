namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Base class for entities that track creation and modification timestamps.
/// </summary>
public abstract class AuditableEntity<TId> : EntityBase<TId>, IAuditable where TId : notnull
{
    /// <summary>
    /// Timestamp indicating when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp indicating when the entity was last modified, or null if never modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
