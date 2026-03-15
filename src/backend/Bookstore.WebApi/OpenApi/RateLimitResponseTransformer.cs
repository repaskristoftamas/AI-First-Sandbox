using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Bookstore.WebApi.OpenApi;

/// <summary>
/// Adds a 429 Too Many Requests response to every operation in the OpenAPI document.
/// </summary>
internal sealed class RateLimitResponseTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Responses?.TryAdd("429", new OpenApiResponse
        {
            Description = "Too Many Requests — rate limit exceeded. Retry after the duration indicated in the Retry-After header."
        });

        return Task.CompletedTask;
    }
}
