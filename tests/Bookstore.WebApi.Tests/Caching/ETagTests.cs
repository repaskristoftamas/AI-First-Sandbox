using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bookstore.WebApi.Tests.Helpers;
using Shouldly;
using Xunit;

namespace Bookstore.WebApi.Tests.Caching;

/// <summary>
/// Integration tests verifying ETag generation and conditional request handling
/// for single-resource GET endpoints.
/// </summary>
public sealed class ETagTests : IAsyncDisposable
{
    private readonly BookstoreWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ETagTests()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.CreateToken("Admin"));
    }

    [Fact]
    public async Task GetAuthorById_ShouldIncludeETagHeader()
    {
        var authorId = await CreateAuthorAsync();

        var response = await _client.GetAsync($"/api/v1/authors/{authorId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
        response.Headers.ETag.IsWeak.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAuthorById_ShouldIncludeCacheControlHeader()
    {
        var authorId = await CreateAuthorAsync();

        var response = await _client.GetAsync($"/api/v1/authors/{authorId}");

        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl.NoCache.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAuthorById_WithMatchingIfNoneMatch_ShouldReturn304()
    {
        var authorId = await CreateAuthorAsync();
        var firstResponse = await _client.GetAsync($"/api/v1/authors/{authorId}");
        var etag = firstResponse.Headers.ETag!.ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/authors/{authorId}");
        request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));
        var secondResponse = await _client.SendAsync(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task GetAuthorById_WithMatchingIfNoneMatch_ShouldReturnEmptyBody()
    {
        var authorId = await CreateAuthorAsync();
        var firstResponse = await _client.GetAsync($"/api/v1/authors/{authorId}");
        var etag = firstResponse.Headers.ETag!.ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/authors/{authorId}");
        request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));
        var secondResponse = await _client.SendAsync(request);

        var body = await secondResponse.Content.ReadAsStringAsync();
        body.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAuthorById_WithMatchingIfNoneMatch_ShouldIncludeETagInResponse()
    {
        var authorId = await CreateAuthorAsync();
        var firstResponse = await _client.GetAsync($"/api/v1/authors/{authorId}");
        var etag = firstResponse.Headers.ETag!;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/authors/{authorId}");
        request.Headers.IfNoneMatch.Add(etag);
        var secondResponse = await _client.SendAsync(request);

        secondResponse.Headers.ETag.ShouldBe(etag);
    }

    [Fact]
    public async Task GetAuthorById_WithNonMatchingIfNoneMatch_ShouldReturn200()
    {
        var authorId = await CreateAuthorAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/authors/{authorId}");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"stale-etag\"", isWeak: true));
        var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAuthorById_WithoutIfNoneMatch_ShouldReturn200WithETag()
    {
        var authorId = await CreateAuthorAsync();

        var response = await _client.GetAsync($"/api/v1/authors/{authorId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAuthorById_AfterUpdate_ETagShouldChange()
    {
        var authorId = await CreateAuthorAsync();
        var firstResponse = await _client.GetAsync($"/api/v1/authors/{authorId}");
        var firstETag = firstResponse.Headers.ETag!;

        await _client.PutAsJsonAsync($"/api/v1/authors/{authorId}", new
        {
            FirstName = "Updated",
            LastName = "Author",
            DateOfBirth = "1990-01-01"
        });

        var secondResponse = await _client.GetAsync($"/api/v1/authors/{authorId}");
        var secondETag = secondResponse.Headers.ETag!;

        secondETag.ShouldNotBe(firstETag);
    }

    [Fact]
    public async Task GetBookById_ShouldIncludeETagHeader()
    {
        var (_, bookId) = await CreateBookWithAuthorAsync();

        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
        response.Headers.ETag.IsWeak.ShouldBeTrue();
    }

    [Fact]
    public async Task GetBookById_WithMatchingIfNoneMatch_ShouldReturn304()
    {
        var (_, bookId) = await CreateBookWithAuthorAsync();
        var firstResponse = await _client.GetAsync($"/api/v1/books/{bookId}");
        var etag = firstResponse.Headers.ETag!.ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/books/{bookId}");
        request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));
        var secondResponse = await _client.SendAsync(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task GetBookById_WithNonMatchingIfNoneMatch_ShouldReturn200()
    {
        var (_, bookId) = await CreateBookWithAuthorAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/books/{bookId}");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"stale-etag\"", isWeak: true));
        var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAuthorById_NotFound_ShouldNotIncludeETag()
    {
        var response = await _client.GetAsync($"/api/v1/authors/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Headers.ETag.ShouldBeNull();
    }

    /// <summary>
    /// Creates an author and returns the new author's identifier.
    /// </summary>
    private async Task<Guid> CreateAuthorAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/authors", new
        {
            FirstName = "Test",
            LastName = "Author",
            DateOfBirth = "1980-06-15"
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>
    /// Creates an author and a book, returning both identifiers.
    /// </summary>
    private async Task<(Guid AuthorId, Guid BookId)> CreateBookWithAuthorAsync()
    {
        var authorId = await CreateAuthorAsync();
        var response = await _client.PostAsJsonAsync("/api/v1/books", new
        {
            Title = "Test Book",
            AuthorId = authorId,
            ISBN = "9780061120084",
            Price = 19.99m,
            PublicationYear = 2020
        });
        response.EnsureSuccessStatusCode();
        var bookId = await response.Content.ReadFromJsonAsync<Guid>();
        return (authorId, bookId);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
