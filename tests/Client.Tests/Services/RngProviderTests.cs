using Client.Services.Physics;

namespace Client.Tests.Services;

public sealed class RngProviderTests
{
    private const int SampleSeed = 12345;

    [Fact]
    public void SameSeedSameSequence()
    {
        IRngProvider first = CreateProvider();
        IRngProvider second = CreateProvider();

        first.Reseed(SampleSeed);
        second.Reseed(SampleSeed);

        float[] firstSequence = GenerateSequence(first, 5);
        float[] secondSequence = GenerateSequence(second, 5);

        Assert.Equal(firstSequence, secondSequence);
    }

    [Fact]
    public void ReseedResetsSequence()
    {
        IRngProvider provider = CreateProvider();
        provider.Reseed(SampleSeed);

        float initialValue = provider.NextFloat();
        provider.Reseed(SampleSeed);
        float reseededValue = provider.NextFloat();

        Assert.Equal(initialValue, reseededValue);
    }

    [Fact]
    public void NextFloatRangeIsWithinUnitInterval()
    {
        IRngProvider provider = CreateProvider();
        provider.Reseed(SampleSeed);

        float value = provider.NextFloat();

        Assert.True(value >= 0f);
        Assert.True(value < 1f);
    }

    [Fact]
    public void CurrentSeedReflectsLatestSeed()
    {
        IRngProvider provider = CreateProvider();
        provider.Reseed(SampleSeed);

        Assert.Equal(SampleSeed, provider.CurrentSeed);
    }

    private static IRngProvider CreateProvider() => new RngProvider();

    private static float[] GenerateSequence(IRngProvider provider, int count)
    {
        float[] values = new float[count];
        for (int index = 0; index < count; index++)
        {
            values[index] = provider.NextFloat();
        }

        return values;
    }
}
