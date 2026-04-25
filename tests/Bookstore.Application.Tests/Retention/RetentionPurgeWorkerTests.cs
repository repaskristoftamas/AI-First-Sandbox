using Bookstore.Infrastructure.Retention;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Retention;

/// <summary>
/// Verifies the <see cref="RetentionPurgeWorker"/> startup behavior: invalid configuration disables
/// the worker gracefully, and sweep failures are tolerated rather than propagated.
/// </summary>
public sealed class RetentionPurgeWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldExitImmediately_WhenOptionsAreInvalid()
    {
        // Arrange
        var scopeFactory = new Mock<IServiceScopeFactory>(MockBehavior.Strict);

        var worker = new RetentionPurgeWorker(
            scopeFactory.Object,
            Options.Create(new RetentionOptions
            {
                RetentionPeriod = TimeSpan.Zero,
                SweepInterval = TimeSpan.Zero
            }),
            NullLogger<RetentionPurgeWorker>.Instance);

        // Act
        using var cts = new CancellationTokenSource();
        await worker.StartAsync(cts.Token);
        await worker.ExecuteTask!;

        // Assert
        scopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInvokePurge_WhenOptionsAreValid()
    {
        // Arrange
        var purgeInvoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var purgeService = new Mock<IRetentionPurgeService>();
        purgeService.Setup(s => s.PurgeAsync(It.IsAny<CancellationToken>()))
            .Callback(() => purgeInvoked.TrySetResult())
            .ReturnsAsync(new RetentionPurgeResult(0, 0, 0));

        var services = new ServiceCollection();
        services.AddScoped(_ => purgeService.Object);
        await using var provider = services.BuildServiceProvider();

        var worker = new RetentionPurgeWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new RetentionOptions
            {
                RetentionPeriod = TimeSpan.FromDays(30),
                SweepInterval = TimeSpan.FromMinutes(10),
                InitialDelay = TimeSpan.Zero
            }),
            NullLogger<RetentionPurgeWorker>.Instance);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await worker.StartAsync(cts.Token);
        await purgeInvoked.Task;

        await cts.CancelAsync();
        await worker.ExecuteTask!;

        // Assert
        purgeService.Verify(s => s.PurgeAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinueLooping_WhenSweepThrows()
    {
        // Arrange
        var secondCallReached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var callCount = 0;

        var purgeService = new Mock<IRetentionPurgeService>();
        purgeService.Setup(s => s.PurgeAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref callCount) >= 2)
                    secondCallReached.TrySetResult();
            })
            .ThrowsAsync(new InvalidOperationException("simulated failure"));

        var services = new ServiceCollection();
        services.AddScoped(_ => purgeService.Object);
        await using var provider = services.BuildServiceProvider();

        var worker = new RetentionPurgeWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new RetentionOptions
            {
                RetentionPeriod = TimeSpan.FromDays(30),
                SweepInterval = TimeSpan.FromMilliseconds(20),
                InitialDelay = TimeSpan.Zero
            }),
            NullLogger<RetentionPurgeWorker>.Instance);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await worker.StartAsync(cts.Token);
        await secondCallReached.Task;

        await cts.CancelAsync();
        await worker.ExecuteTask!;

        // Assert — worker kept running after a failure (called more than once) and stopped on cancellation.
        purgeService.Invocations.Count.ShouldBeGreaterThanOrEqualTo(2);
    }
}
