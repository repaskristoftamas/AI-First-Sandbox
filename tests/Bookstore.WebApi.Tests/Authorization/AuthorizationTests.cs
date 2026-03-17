using System.Net;
using System.Net.Http.Headers;
using Bookstore.WebApi.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Bookstore.WebApi.Tests.Authorization;

/// <summary>
/// Integration tests verifying authorization behavior for endpoints protected by role-based policies.
/// DELETE endpoints require the AdminOnly policy (Admin role); other write endpoints require authentication only.
/// </summary>
public sealed class AuthorizationTests(AuthorizationTestFactory factory)
    : IClassFixture<AuthorizationTestFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/api/v1/books")]
    [InlineData("/api/v1/authors")]
    public async Task DeleteEndpoint_WithoutToken_ShouldReturn401(string baseUrl)
    {
        var response = await _client.DeleteAsync($"{baseUrl}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/books")]
    [InlineData("/api/v1/authors")]
    public async Task DeleteEndpoint_WithTokenButNoAdminRole_ShouldReturn403(string baseUrl)
    {
        SetBearerToken();

        var response = await _client.DeleteAsync($"{baseUrl}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/v1/books")]
    [InlineData("/api/v1/authors")]
    public async Task DeleteEndpoint_WithAdminRole_ShouldReturn404ForNonExistentResource(string baseUrl)
    {
        SetBearerToken("Admin");

        var response = await _client.DeleteAsync($"{baseUrl}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("POST", "/api/v1/books")]
    [InlineData("PUT", "/api/v1/books")]
    [InlineData("POST", "/api/v1/authors")]
    [InlineData("PUT", "/api/v1/authors")]
    public async Task WriteEndpoint_WithoutToken_ShouldReturn401(string method, string baseUrl)
    {
        var url = method == "PUT" ? $"{baseUrl}/{Guid.NewGuid()}" : baseUrl;
        var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("POST", "/api/v1/books")]
    [InlineData("PUT", "/api/v1/books")]
    [InlineData("POST", "/api/v1/authors")]
    [InlineData("PUT", "/api/v1/authors")]
    public async Task WriteEndpoint_WithTokenButNoAdminRole_ShouldNotReturn401Or403(string method, string baseUrl)
    {
        SetBearerToken();

        var url = method == "PUT" ? $"{baseUrl}/{Guid.NewGuid()}" : baseUrl;
        var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.ShouldNotBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/v1/books")]
    [InlineData("/api/v1/authors")]
    public async Task GetEndpoint_WithoutToken_ShouldReturn200(string baseUrl)
    {
        var response = await _client.GetAsync(baseUrl);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Sets the Authorization header on the client with a JWT token containing the specified roles.
    /// </summary>
    private void SetBearerToken(params string[] roles)
    {
        var token = JwtTokenHelper.CreateToken(roles);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }
}
