using System.Numerics;

namespace Client.Diagnostics;

public sealed class DebugOverlayService
{
    private readonly TimeSpan _frameBudget;
    private readonly int _sampleWindowSize;
    private readonly Queue<double> _frameSamples;
    private double _frameSumMilliseconds;

    public DebugOverlayService(TimeSpan frameBudget, int sampleWindowSize)
    {
        if (frameBudget <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(frameBudget), frameBudget, "Frame budget must be greater than zero.");
        }

        if (sampleWindowSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleWindowSize), sampleWindowSize, "Sample window size must be positive.");
        }

        _frameBudget = frameBudget;
        _sampleWindowSize = sampleWindowSize;
        _frameSamples = new Queue<double>(sampleWindowSize);
    }

    public TimeSpan AverageFrameTime => _frameSamples.Count == 0
        ? TimeSpan.Zero
        : TimeSpan.FromMilliseconds(_frameSumMilliseconds / _frameSamples.Count);

    public double AverageFps
    {
        get
        {
            double averageFrameMilliseconds = AverageFrameTime.TotalMilliseconds;

            return averageFrameMilliseconds <= double.Epsilon
                ? 0d
                : 1000d / averageFrameMilliseconds;
        }
    }

    public bool IsBudgetExceeded => AverageFrameTime > _frameBudget;

    public int SampleCount => _frameSamples.Count;

    public int CollisionCount { get; private set; }

    public Vector2? LastCollisionPosition { get; private set; }

    public DateTimeOffset? LastCollisionTimestamp { get; private set; }

    public void RecordFrame(TimeSpan frameDuration)
    {
        if (frameDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(frameDuration), frameDuration, "Frame duration cannot be negative.");
        }

        double durationMilliseconds = frameDuration.TotalMilliseconds;

        _frameSamples.Enqueue(durationMilliseconds);
        _frameSumMilliseconds += durationMilliseconds;

        if (_frameSamples.Count > _sampleWindowSize)
        {
            double removed = _frameSamples.Dequeue();
            _frameSumMilliseconds -= removed;
        }
    }

    public void RecordCollision(Vector2 position, DateTimeOffset timestamp)
    {
        LastCollisionPosition = position;
        LastCollisionTimestamp = timestamp;
        CollisionCount++;
    }

    public void Reset()
    {
        _frameSamples.Clear();
        _frameSumMilliseconds = 0d;
        CollisionCount = 0;
        LastCollisionPosition = null;
        LastCollisionTimestamp = null;
    }
}
