using Bookstore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookstore.Infrastructure.Retention;

/// <summary>
/// Default implementation of <see cref="IRetentionPurgeService"/> backed by EF Core.
/// </summary>
public sealed class RetentionPurgeService(
    BookstoreDbContext context,
    IOptions<RetentionOptions> options,
    TimeProvider timeProvider,
    ILogger<RetentionPurgeService> logger) : IRetentionPurgeService
{
    /// <inheritdoc />
    public async Task<RetentionPurgeResult> PurgeAsync(CancellationToken cancellationToken)
    {
        var retentionPeriod = options.Value.RetentionPeriod;
        var cutoff = timeProvider.GetUtcNow() - retentionPeriod;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Books are purged before Authors to respect the Book→Author FK with DeleteBehavior.Restrict.
        var booksPurged = await context.Books
            .IgnoreQueryFilters()
            .Where(b => b.DeletedAt != null && b.DeletedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var authorsPurged = await context.Authors
            .IgnoreQueryFilters()
            .Where(a => a.DeletedAt != null && a.DeletedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var usersPurged = await context.Users
            .IgnoreQueryFilters()
            .Where(u => u.DeletedAt != null && u.DeletedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Retention purge completed. Cutoff: {Cutoff}. Books: {Books}, Authors: {Authors}, Users: {Users}.",
            cutoff, booksPurged, authorsPurged, usersPurged);

        return new RetentionPurgeResult(booksPurged, authorsPurged, usersPurged);
    }
}
