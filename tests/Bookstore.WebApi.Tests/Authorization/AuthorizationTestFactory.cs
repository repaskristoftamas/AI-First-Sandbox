using Bookstore.WebApi.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace Bookstore.WebApi.Tests.Authorization;

/// <summary>
/// Factory for authorization integration tests that raises rate limits to prevent
/// interference when sharing a single host across many authenticated requests.
/// </summary>
public sealed class AuthorizationTestFactory : BookstoreWebApplicationFactory
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("RateLimiting:Anonymous:PermitLimit", "1000");
        builder.UseSetting("RateLimiting:Authenticated:PermitLimit", "1000");
    }
}
