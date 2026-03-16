using Bookstore.Application.Abstractions;
using Bookstore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Infrastructure;

/// <summary>
/// Registers infrastructure-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the EF Core database context and its abstraction to the service collection.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Application configuration used to read connection strings and provider settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";

        services.AddDbContext<BookstoreDbContext>(options =>
            ConfigureProvider(options, provider, configuration));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<BookstoreDbContext>());

        return services;
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the database.
    /// </summary>
    /// <param name="services">The application's root service provider.</param>
    /// <param name="cancellationToken">Token to cancel the migration if the host is stopping.</param>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        if (dbContext.Database.IsRelational())
            await dbContext.Database.MigrateAsync(cancellationToken);
        else
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <summary>
    /// Configures the EF Core database provider based on the specified provider name.
    /// </summary>
    /// <param name="options">The options builder to configure.</param>
    /// <param name="provider">The provider name (<c>SqlServer</c> or <c>PostgreSQL</c>).</param>
    /// <param name="configuration">Application configuration for reading connection strings.</param>
    internal static void ConfigureProvider(
        DbContextOptionsBuilder options,
        string provider,
        IConfiguration configuration)
    {
        switch (provider)
        {
            case "SqlServer":
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                break;
            case "PostgreSQL":
                options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported database provider: '{provider}'. Use 'SqlServer' or 'PostgreSQL'.");
        }
    }
}
