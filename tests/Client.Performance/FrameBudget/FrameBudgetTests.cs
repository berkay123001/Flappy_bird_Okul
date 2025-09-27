using Client.Diagnostics;

namespace Client.Performance.FrameBudget;

public sealed class FrameBudgetTests
{
    private static readonly TimeSpan FrameBudget = TimeSpan.FromMilliseconds(16.7);
    private const int SampleWindow = 120;

    [Fact]
    public void FramesUnderBudgetRemainHealthy()
    {
        DebugOverlayService service = CreateService();

        foreach (double frameMs in new[] { 10d, 12.5d, 14d, 11.8d, 16.6d })
        {
            service.RecordFrame(TimeSpan.FromMilliseconds(frameMs));
        }

        Assert.False(service.IsBudgetExceeded);
        Assert.InRange(service.AverageFrameTime.TotalMilliseconds, 0d, FrameBudget.TotalMilliseconds);
        Assert.InRange(service.AverageFps, 58d, 120d);
    }

    [Fact]
    public void BudgetExceededWhenWindowAverageCrossesThreshold()
    {
        DebugOverlayService service = CreateService();

        foreach (double frameMs in Enumerable.Repeat(15d, SampleWindow / 2))
        {
            service.RecordFrame(TimeSpan.FromMilliseconds(frameMs));
        }

        foreach (double frameMs in Enumerable.Repeat(22d, SampleWindow / 2))
        {
            service.RecordFrame(TimeSpan.FromMilliseconds(frameMs));
        }

        Assert.True(service.IsBudgetExceeded);
        Assert.True(service.AverageFrameTime.TotalMilliseconds > FrameBudget.TotalMilliseconds);
    }

    [Fact]
    public void ResetClearsHistoricalSamples()
    {
        DebugOverlayService service = CreateService();

        service.RecordFrame(TimeSpan.FromMilliseconds(30d));
        Assert.True(service.IsBudgetExceeded);

        service.Reset();

        service.RecordFrame(TimeSpan.FromMilliseconds(12d));
        Assert.False(service.IsBudgetExceeded);
        Assert.InRange(service.AverageFrameTime.TotalMilliseconds, 0d, FrameBudget.TotalMilliseconds);
    }

    private static DebugOverlayService CreateService() =>
        new(frameBudget: FrameBudget, sampleWindowSize: SampleWindow);
}
