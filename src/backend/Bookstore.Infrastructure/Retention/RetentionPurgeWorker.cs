using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookstore.Infrastructure.Retention;

/// <summary>
/// Hosted background service that periodically invokes <see cref="IRetentionPurgeService"/>
/// to permanently delete records past their retention window.
/// </summary>
/// <remarks>
/// Uses <see cref="IServiceScopeFactory"/> to resolve the scoped <see cref="IRetentionPurgeService"/>
/// once per sweep, since the worker itself is registered as a singleton hosted service.
/// </remarks>
public sealed class RetentionPurgeWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<RetentionOptions> options,
    ILogger<RetentionPurgeWorker> logger) : BackgroundService
{
    /// <summary>
    /// Runs the purge loop until the host requests shutdown.
    /// </summary>
    /// <remarks>
    /// Exits immediately if <see cref="RetentionOptions"/> is invalid, logging an error and leaving the host running.
    /// Each sweep creates a dedicated DI scope and tolerates individual failures by logging and waiting the full interval.
    /// </remarks>
    /// <param name="stoppingToken">Token raised when the host is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = options.Value;

        if (!config.IsValid)
        {
            logger.LogError(
                "Retention worker disabled: RetentionPeriod ({RetentionPeriod}) and SweepInterval ({SweepInterval}) must both be greater than zero.",
                config.RetentionPeriod,
                config.SweepInterval);
            return;
        }

        logger.LogInformation(
            "Retention worker starting. RetentionPeriod: {RetentionPeriod}, SweepInterval: {SweepInterval}, InitialDelay: {InitialDelay}.",
            config.RetentionPeriod,
            config.SweepInterval,
            config.InitialDelay);

        try
        {
            if (config.InitialDelay > TimeSpan.Zero)
                await Task.Delay(config.InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunSweepAsync(stoppingToken);
                await Task.Delay(config.SweepInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Retention worker stopping due to host shutdown.");
        }
    }

    /// <summary>
    /// Executes a single purge sweep inside a fresh DI scope, swallowing expected exceptions for the outer loop.
    /// </summary>
    private async Task RunSweepAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IRetentionPurgeService>();
            await service.PurgeAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Retention purge sweep failed. Retrying after the configured interval.");
        }
    }
}
