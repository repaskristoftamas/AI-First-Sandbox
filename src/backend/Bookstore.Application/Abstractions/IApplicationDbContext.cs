using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Abstractions;

/// <summary>
/// Abstraction over the database context, exposing only the sets and operations needed by the application layer.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Queryable set of books in the catalog.
    /// </summary>
    DbSet<Book> Books { get; }

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
