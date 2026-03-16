using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure;

/// <summary>
/// Centralises database provider names, connection-string key mapping, and EF Core provider configuration.
/// </summary>
internal static class DatabaseProviderMap
{
    /// <summary>The provider name for SQL Server.</summary>
    internal const string SqlServer = "SqlServer";

    /// <summary>The provider name for PostgreSQL.</summary>
    internal const string PostgreSQL = "PostgreSQL";

    /// <summary>
    /// Returns the connection-string configuration key for the given provider.
    /// </summary>
    internal static string GetConnectionStringKey(string provider) => provider switch
    {
        SqlServer => "DefaultConnection",
        PostgreSQL => "PostgreSQL",
        _ => throw new InvalidOperationException(
            $"Unsupported database provider: '{provider}'. Use '{SqlServer}' or '{PostgreSQL}'.")
    };

    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> with the correct EF Core provider.
    /// </summary>
    internal static void Configure(
        DbContextOptionsBuilder options,
        string provider,
        string connectionString)
    {
        switch (provider)
        {
            case SqlServer:
                options.UseSqlServer(connectionString);
                break;
            case PostgreSQL:
                options.UseNpgsql(connectionString);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported database provider: '{provider}'. Use '{SqlServer}' or '{PostgreSQL}'.");
        }
    }
}
