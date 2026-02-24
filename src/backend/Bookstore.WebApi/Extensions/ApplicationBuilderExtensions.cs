using Bookstore.WebApi.Endpoints;

namespace Bookstore.WebApi.Extensions;

/// <summary>
/// Extension methods for registering endpoint definitions at application startup.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Discovers and registers all <see cref="IEndpointDefinition"/> implementations from the WebApi assembly.
    /// </summary>
    public static IApplicationBuilder RegisterEndpointDefinitions(this WebApplication app)
    {
        var endpointDefinitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>();

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.RegisterEndpoints(app);
        }

        return app;
    }
}
