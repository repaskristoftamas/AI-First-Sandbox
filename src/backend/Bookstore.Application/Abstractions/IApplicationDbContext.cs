using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Book> Books { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
