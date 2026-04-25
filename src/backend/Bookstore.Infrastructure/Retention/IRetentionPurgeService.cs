namespace Bookstore.Infrastructure.Retention;

/// <summary>
/// Permanently deletes soft-deleted records whose retention period has elapsed.
/// </summary>
public interface IRetentionPurgeService
{
    /// <summary>
    /// Permanently deletes all soft-deleted records older than the configured retention period.
    /// </summary>
    /// <remarks>
    /// Deletes books before authors to respect the <c>Book → Author</c> foreign key with <c>DeleteBehavior.Restrict</c>.
    /// Uses bulk <c>ExecuteDeleteAsync</c> to bypass the change tracker and the soft-delete guard in <c>SaveChangesAsync</c>.
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The per-entity count of records purged.</returns>
    Task<RetentionPurgeResult> PurgeAsync(CancellationToken cancellationToken);
}
