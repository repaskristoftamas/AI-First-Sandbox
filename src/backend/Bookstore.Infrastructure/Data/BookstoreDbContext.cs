using Bookstore.Application.Abstractions;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.Domain.Users;
using Bookstore.SharedKernel.Abstractions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Infrastructure.Data;

/// <summary>
/// EF Core database context for the bookstore.
/// </summary>
/// <remarks>
/// Implements automatic audit timestamp tracking for entities that implement <see cref="IAuditable"/>.
/// </remarks>
public sealed class BookstoreDbContext(DbContextOptions<BookstoreDbContext> options, TimeProvider timeProvider, IPublisher publisher) : DbContext(options), IApplicationDbContext
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
    /// Queryable set of users persisted in the data store.
    /// </summary>
    public DbSet<User> Users => Set<User>();

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
    /// Applies model-wide conventions before individual entity configurations run.
    /// </summary>
    /// <remarks>
    /// SQLite has no native <see cref="DateTimeOffset"/> type and the default string converter
    /// prevents range comparisons from being translated to SQL. Using the binary (ticks)
    /// converter makes comparisons work in tests while leaving SQL Server's native
    /// <c>datetimeoffset</c> storage untouched in production.
    /// </remarks>
    /// <param name="configurationBuilder">Builder used to configure conventions applied to the model.</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            configurationBuilder
                .Properties<DateTimeOffset>()
                .HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }

    /// <summary>
    /// Persists all pending changes to the data store and dispatches domain events.
    /// </summary>
    /// <remarks>
    /// Automatically stamps audit timestamps on added and modified entities before saving.
    /// Domain events are collected before save and dispatched after successful persistence
    /// to ensure events only fire when data is committed.
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceSoftDelete();

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

        var entries = ChangeTracker.Entries<IHasDomainEvents>().ToList();

        var domainEvents = entries
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in entries)
        {
            entry.Entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Prevents hard deletion of soft-deletable entities by inspecting the change tracker.
    /// </summary>
    /// <remarks>
    /// Soft-deletable entities must be removed via their <c>Delete</c> domain method, which
    /// sets <see cref="ISoftDeletable.IsDeleted"/>. Calling <c>DbSet.Remove</c> bypasses
    /// domain events and tombstone state, so it is blocked here.
    /// </remarks>
    private void EnforceSoftDelete()
    {
        var hardDeleted = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        if (hardDeleted.Count > 0)
        {
            var entityNames = string.Join(", ", hardDeleted.Select(e => e.Entity.GetType().Name));
            throw new InvalidOperationException(
                $"Hard delete of soft-deletable entities is not allowed: [{entityNames}]. Use the entity's Delete method to perform a soft delete.");
        }
    }
}
