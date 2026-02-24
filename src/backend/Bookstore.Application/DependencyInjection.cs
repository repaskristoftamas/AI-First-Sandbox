using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Application;

/// <summary>
/// Registers application-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Mediator and application-layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
