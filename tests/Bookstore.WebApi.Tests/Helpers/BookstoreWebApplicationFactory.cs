using Bookstore.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.WebApi.Tests.Helpers;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces SQL Server with an in-memory database
/// and uses the Testing environment for test-specific configuration.
/// </summary>
public sealed class BookstoreWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<BookstoreDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<BookstoreDbContext>(options =>
                options.UseInMemoryDatabase($"BookstoreTest-{Guid.NewGuid()}"));
        });
    }
}
