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
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
