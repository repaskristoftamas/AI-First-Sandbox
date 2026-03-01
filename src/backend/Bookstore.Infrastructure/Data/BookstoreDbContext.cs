using Bookstore.Application.Abstractions;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Data;

/// <summary>
/// EF Core database context for the bookstore.
/// </summary>
/// <remarks>
/// Implements automatic audit timestamp tracking for entities that implement <see cref="IAuditable"/>.
/// </remarks>
public sealed class BookstoreDbContext(DbContextOptions<BookstoreDbContext> options, TimeProvider timeProvider) : DbContext(options), IApplicationDbContext
{
    /// <summary>
    /// Queryable set of authors persisted in the data store.
    /// </summary>
    public DbSet<Author> Authors => Set<Author>();

    /// <summary>
    /// Queryable set of books persisted in the data store.
    /// </summary>
    public DbSet<Book> Books => Set<Book>();

    /// <summary>
    /// Applies entity configurations from the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookstoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists all pending changes to the data store.
    /// </summary>
    /// <remarks>
    /// Automatically stamps audit timestamps on added and modified entities before saving.
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
