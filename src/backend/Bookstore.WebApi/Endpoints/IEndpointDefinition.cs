namespace Bookstore.WebApi.Endpoints;

public interface IEndpointDefinition
{
    void RegisterEndpoints(IEndpointRouteBuilder app);
}
