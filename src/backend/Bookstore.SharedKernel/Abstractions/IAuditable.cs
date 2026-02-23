namespace Bookstore.SharedKernel.Abstractions;

/// <summary>
/// Contract for entities that support automatic audit timestamp tracking.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Timestamp indicating when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp indicating when the entity was last modified, or null if never modified.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}
