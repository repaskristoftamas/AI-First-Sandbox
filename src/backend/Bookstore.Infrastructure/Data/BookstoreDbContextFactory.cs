using Mediator;
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
        var provider = Environment.GetEnvironmentVariable("DatabaseProvider") ?? "SqlServer";
        var connectionString = GetConnectionString(provider);

        var optionsBuilder = new DbContextOptionsBuilder<BookstoreDbContext>();

        switch (provider)
        {
            case "SqlServer":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "PostgreSQL":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported database provider: '{provider}'. Use 'SqlServer' or 'PostgreSQL'.");
        }

        return new BookstoreDbContext(optionsBuilder.Options, TimeProvider.System, new DesignTimePublisher());
    }

    /// <summary>
    /// Reads the connection string from the environment variable matching the given provider.
    /// </summary>
    private static string GetConnectionString(string provider)
    {
        var envVarName = provider switch
        {
            "SqlServer" => "ConnectionStrings__DefaultConnection",
            "PostgreSQL" => "ConnectionStrings__PostgreSQL",
            _ => throw new InvalidOperationException(
                $"Unsupported database provider: '{provider}'. Use 'SqlServer' or 'PostgreSQL'.")
        };

        return Environment.GetEnvironmentVariable(envVarName)
            ?? throw new InvalidOperationException(
                $"Set the '{envVarName}' environment variable before running EF tooling.");
    }

    /// <summary>
    /// No-op publisher used only during design-time EF Core tooling (migrations).
    /// </summary>
    private sealed class DesignTimePublisher : IPublisher
    {
        /// <inheritdoc />
        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => ValueTask.CompletedTask;

        /// <inheritdoc />
        public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
