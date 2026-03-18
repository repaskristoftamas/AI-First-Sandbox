using System.IO.Hashing;
using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace Bookstore.WebApi.Filters;

/// <summary>
/// Endpoint filter that generates weak ETags from response content and handles
/// conditional requests via the <c>If-None-Match</c> header, returning <c>304 Not Modified</c>
/// when the client's cached version is still current.
/// </summary>
public sealed class ETagEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        var innerResult = result is INestedHttpResult nested ? nested.Result : result;

        if (innerResult is not (IValueHttpResult { Value: not null } valueResult
            and IStatusCodeHttpResult { StatusCode: StatusCodes.Status200OK }))
        {
            return result;
        }

        var etag = GenerateETag(valueResult.Value);
        var httpContext = context.HttpContext;

        httpContext.Response.Headers.ETag = etag.ToString();
        httpContext.Response.Headers.CacheControl = "no-cache";

        var ifNoneMatch = httpContext.Request.GetTypedHeaders().IfNoneMatch;
        if (ifNoneMatch.Any(e => e.Compare(etag, useStrongComparison: false)))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified);
        }

        return result;
    }

    /// <summary>
    /// Generates a weak ETag by computing an XxHash3 fingerprint of the JSON-serialized response value.
    /// </summary>
    private static EntityTagHeaderValue GenerateETag(object value)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType());
        var hash = XxHash3.Hash(bytes);
        return new EntityTagHeaderValue($"\"{Convert.ToHexString(hash)}\"", isWeak: true);
    }
}
