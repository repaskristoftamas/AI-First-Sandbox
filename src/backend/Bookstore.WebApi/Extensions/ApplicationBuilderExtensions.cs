using Bookstore.WebApi.Endpoints;

namespace Bookstore.WebApi.Extensions;

/// <summary>
/// Extension methods for registering endpoint definitions at application startup.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Resolves all registered <see cref="IEndpointDefinition"/> implementations from DI
    /// and maps their routes onto the application.
    /// </summary>
    /// <param name="app">The web application to register endpoints on.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder RegisterEndpointDefinitions(this WebApplication app)
    {
        foreach (var definition in app.Services.GetRequiredService<IEnumerable<IEndpointDefinition>>())
            definition.RegisterEndpoints(app);

        return app;
    }
}
