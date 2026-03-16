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
public class BookstoreWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BookstoreDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<DbContextOptions<BookstoreDbContext>>(_ =>
                new DbContextOptionsBuilder<BookstoreDbContext>()
                    .UseInMemoryDatabase($"BookstoreTest-{Guid.NewGuid()}")
                    .Options);
        });
    }
}
