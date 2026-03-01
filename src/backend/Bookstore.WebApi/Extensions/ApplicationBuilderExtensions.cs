using Bookstore.WebApi.Endpoints.Authors;
using Bookstore.WebApi.Endpoints.Books;

namespace Bookstore.WebApi.Extensions;

/// <summary>
/// Extension methods for registering endpoint definitions at application startup.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers all endpoint definitions with explicit, deterministic ordering.
    /// </summary>
    /// <param name="app">The web application to register endpoints on.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder RegisterEndpointDefinitions(this WebApplication app)
    {
        new AuthorEndpoints().RegisterEndpoints(app);
        new BookEndpoints().RegisterEndpoints(app);

        return app;
    }
}
