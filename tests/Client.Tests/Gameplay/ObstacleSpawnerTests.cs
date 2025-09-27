using Client.Models;
using Client.Services;
using Client.Services.Physics;

namespace Client.Tests.Gameplay;

public sealed class ObstacleSpawnerTests
{
    [Fact]
    public void GapSizeFallsWithinConfiguredBounds()
    {
        var rng = RecordingRng.WithValues(0.25f);
        ObstacleSpawner spawner = new(rng);

        PipePair pipe = spawner.SpawnNext();

        Assert.InRange(pipe.GapSize, ObstacleSpawner.MinimumGap, ObstacleSpawner.MaximumGap);
    }

    [Fact]
    public void SpawnUsesRngPerInvocation()
    {
        var rng = RecordingRng.WithValues(0.1f, 0.9f, 0.2f, 0.8f);
        ObstacleSpawner spawner = new(rng);

        PipePair first = spawner.SpawnNext();
        PipePair second = spawner.SpawnNext();

        Assert.Equal(4, rng.NextFloatCount);
        Assert.NotEqual(first.GapCenterY, second.GapCenterY);
    }

    [Fact]
    public void SpawnAssignsConsistentHorizontalSpeed()
    {
        var rng = RecordingRng.WithValues(0.5f);
        ObstacleSpawner spawner = new(rng);

        PipePair pipe = spawner.SpawnNext();

        Assert.Equal(ObstacleSpawner.PipeSpeed, pipe.VelocityX, 3);
        Assert.Equal(ObstacleSpawner.SpawnX, pipe.PositionX, 3);
        Assert.False(pipe.Passed);
    }

    [Fact]
    public void GapCenterRespectsPlayfieldBounds()
    {
        var rng = RecordingRng.WithValues(0.75f);
        ObstacleSpawner spawner = new(rng);

        PipePair pipe = spawner.SpawnNext();
        float halfGap = pipe.GapSize / 2f;

        Assert.InRange(pipe.GapCenterY, halfGap, ObstacleSpawner.PlayfieldHeight - halfGap);
    }

    private sealed class RecordingRng : IRngProvider
    {
        private readonly Queue<float> _values = new();

        private RecordingRng()
        {
        }

        public int CurrentSeed { get; private set; }

        public int NextFloatCount { get; private set; }

        public static RecordingRng WithValues(params float[] values)
        {
            RecordingRng rng = new();
            foreach (float value in values)
            {
                rng._values.Enqueue(value);
            }

            return rng;
        }

        public void Reseed(int seed) => CurrentSeed = seed;

        public float NextFloat()
        {
            NextFloatCount++;
            return _values.Count > 0 ? _values.Dequeue() : 0f;
        }
    }
}
