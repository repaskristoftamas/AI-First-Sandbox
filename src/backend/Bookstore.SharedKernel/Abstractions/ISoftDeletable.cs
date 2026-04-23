namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Contract for entities that support soft deletion.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Timestamp indicating when the entity was soft-deleted, or null if it has not been deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }
}
