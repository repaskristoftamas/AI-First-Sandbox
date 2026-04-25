using Bookstore.Infrastructure.Retention;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Retention;

/// <summary>
/// Verifies the validation logic on <see cref="RetentionOptions"/> used to disable the worker
/// when misconfigured.
/// </summary>
public sealed class RetentionOptionsTests
{
    [Fact]
    public void IsValid_ShouldBeTrue_WhenBothValuesArePositive()
    {
        var options = new RetentionOptions
        {
            RetentionPeriod = TimeSpan.FromDays(30),
            SweepInterval = TimeSpan.FromHours(1)
        };

        options.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(30, 0)]
    [InlineData(0, 0)]
    [InlineData(-1, 60)]
    [InlineData(30, -1)]
    public void IsValid_ShouldBeFalse_WhenEitherValueIsNotPositive(int retentionDays, int sweepMinutes)
    {
        var options = new RetentionOptions
        {
            RetentionPeriod = TimeSpan.FromDays(retentionDays),
            SweepInterval = TimeSpan.FromMinutes(sweepMinutes)
        };

        options.IsValid.ShouldBeFalse();
    }
}
