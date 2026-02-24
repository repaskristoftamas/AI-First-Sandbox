using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Data;

/// <summary>
/// EF Core database context for the bookstore, implementing automatic audit timestamp tracking.
/// </summary>
public sealed class BookstoreDbContext(DbContextOptions<BookstoreDbContext> options) : DbContext(options), IApplicationDbContext
{
    /// <summary>
    /// Queryable set of books persisted in the data store.
    /// </summary>
    public DbSet<Book> Books => Set<Book>();

    /// <summary>
    /// Applies entity configurations from the infrastructure assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookstoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists changes while automatically stamping audit timestamps on added and modified entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

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
