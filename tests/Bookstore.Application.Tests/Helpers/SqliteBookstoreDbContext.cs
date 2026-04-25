using Bookstore.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bookstore.Application.Tests.Helpers;

/// <summary>
/// Test-only <see cref="BookstoreDbContext"/> subclass that applies SQLite-specific conventions.
/// </summary>
/// <remarks>
/// SQLite has no native <see cref="DateTimeOffset"/> type and the default string converter
/// prevents range comparisons from being translated to SQL. Using the binary (ticks)
/// converter makes comparisons work in tests while leaving SQL Server's native
/// <c>datetimeoffset</c> storage untouched in production.
/// </remarks>
public sealed class SqliteBookstoreDbContext(
    DbContextOptions<BookstoreDbContext> options,
    TimeProvider timeProvider,
    IPublisher publisher) : BookstoreDbContext(options, timeProvider, publisher)
{
    /// <summary>
    /// Applies the <see cref="DateTimeOffsetToBinaryConverter"/> so that range comparisons
    /// on <see cref="DateTimeOffset"/> columns translate correctly under SQLite.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }
}
