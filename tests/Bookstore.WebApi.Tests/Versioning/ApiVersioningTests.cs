using System.Net;
using Bookstore.WebApi.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Bookstore.WebApi.Tests.Versioning;

/// <summary>
/// Integration tests verifying the API versioning contract:
/// versioned paths resolve correctly and unversioned paths are not routable.
/// </summary>
public sealed class ApiVersioningTests : IAsyncDisposable
{
    private readonly BookstoreWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ApiVersioningTests()
    {
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/authors")]
    [InlineData("/api/books")]
    public async Task UnversionedPath_ShouldReturnNotFound(string path)
    {
        var response = await _client.GetAsync(path);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/api/v1/authors")]
    [InlineData("/api/v1/books")]
    public async Task VersionedPath_ShouldIncludeApiSupportedVersionsHeader(string path)
    {
        var response = await _client.GetAsync(path);

        response.Headers.Contains("api-supported-versions").ShouldBeTrue();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
