namespace Bookstore.WebApi.Endpoints;

/// <summary>
/// Contract for classes that define a group of minimal API endpoints to be auto-registered at startup.
/// </summary>
public interface IEndpointDefinition
{
    /// <summary>
    /// Maps the endpoint routes onto the application's route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder to register routes on.</param>
    void RegisterEndpoints(IEndpointRouteBuilder app);
}
