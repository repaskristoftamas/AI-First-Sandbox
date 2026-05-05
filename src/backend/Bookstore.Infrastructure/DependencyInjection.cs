using Bookstore.Application.Abstractions;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Identity;
using Bookstore.Infrastructure.Retention;
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
        var provider = configuration["DatabaseProvider"] ?? DatabaseProviderMap.SqlServer;
        var connectionString = configuration.GetConnectionString(DatabaseProviderMap.GetConnectionStringKey(provider));

        services.AddDbContext<BookstoreDbContext>(options =>
            DatabaseProviderMap.Configure(options, provider, connectionString!));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<BookstoreDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddOptions<RetentionOptions>()
            .Bind(configuration.GetSection(RetentionOptions.SectionName));

        services.AddScoped<IRetentionPurgeService, RetentionPurgeService>();
        services.AddHostedService<RetentionPurgeWorker>();

        services.AddHealthChecks()
            .AddDbContextCheck<BookstoreDbContext>(tags: ["ready"]);

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

}
