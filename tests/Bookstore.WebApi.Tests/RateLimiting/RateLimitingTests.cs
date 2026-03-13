using System.Net;
using Bookstore.WebApi.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Bookstore.WebApi.Tests.RateLimiting;

/// <summary>
/// Integration tests verifying rate limiting behavior for the API.
/// Uses appsettings.Testing.json with a permit limit of 3 for anonymous requests.
/// </summary>
public sealed class RateLimitingTests : IDisposable
{
    private readonly BookstoreWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public RateLimitingTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AnonymousRequest_UnderLimit_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/authors");

        response.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task AnonymousRequest_ExceedingLimit_ShouldReturn429()
    {
        for (var i = 0; i < 3; i++)
            await _client.GetAsync("/api/authors");

        var response = await _client.GetAsync("/api/authors");

        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task AnonymousRequest_ExceedingLimit_ShouldIncludeRetryAfterHeader()
    {
        for (var i = 0; i < 3; i++)
            await _client.GetAsync("/api/authors");

        var response = await _client.GetAsync("/api/authors");

        response.Headers.RetryAfter.ShouldNotBeNull();
    }

    [Fact]
    public async Task AnonymousRequest_ExceedingLimit_ShouldReturnProblemDetails()
    {
        for (var i = 0; i < 3; i++)
            await _client.GetAsync("/api/authors");

        var response = await _client.GetAsync("/api/authors");
        var content = await response.Content.ReadAsStringAsync();

        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        content.ShouldContain("Too Many Requests");
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
