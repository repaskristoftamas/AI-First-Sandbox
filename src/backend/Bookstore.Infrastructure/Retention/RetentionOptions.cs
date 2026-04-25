namespace Bookstore.Infrastructure.Retention;

/// <summary>
/// Options controlling the retention-based permanent deletion of soft-deleted records.
/// </summary>
public sealed class RetentionOptions
{
    /// <summary>
    /// Configuration section name used to bind these options from <c>appsettings.json</c>.
    /// </summary>
    public const string SectionName = "Retention";

    /// <summary>
    /// How long a soft-deleted record is retained before it becomes eligible for permanent deletion.
    /// </summary>
    public TimeSpan RetentionPeriod { get; init; }

    /// <summary>
    /// How often the background worker sweeps the database for records to purge.
    /// </summary>
    public TimeSpan SweepInterval { get; init; }

    /// <summary>
    /// Delay between application startup and the first purge sweep. Allows the app to finish bootstrapping.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Indicates whether the configured values can drive a functioning retention worker.
    /// </summary>
    public bool IsValid => RetentionPeriod > TimeSpan.Zero && SweepInterval > TimeSpan.Zero;
}
