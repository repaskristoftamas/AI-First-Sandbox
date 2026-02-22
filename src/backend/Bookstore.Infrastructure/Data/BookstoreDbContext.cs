using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Data;

public sealed class BookstoreDbContext : DbContext, IApplicationDbContext
{
    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookstoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
