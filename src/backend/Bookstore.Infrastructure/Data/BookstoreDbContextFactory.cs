using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookstore.Infrastructure.Data;

/// <summary>
/// Design-time factory for <see cref="BookstoreDbContext"/>, used by EF Core tooling.
/// </summary>
internal sealed class BookstoreDbContextFactory : IDesignTimeDbContextFactory<BookstoreDbContext>
{
    /// <summary>
    /// Creates a <see cref="BookstoreDbContext"/> instance for design-time operations such as migrations.
    /// </summary>
    /// <param name="args">Command-line arguments (not used).</param>
    /// <returns>A configured <see cref="BookstoreDbContext"/> instance.</returns>
    public BookstoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "Set the 'ConnectionStrings__DefaultConnection' environment variable before running EF tooling.");

        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BookstoreDbContext(options);
    }
}
