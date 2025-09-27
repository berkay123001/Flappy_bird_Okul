namespace Client.Services.Physics;

public sealed class RngProvider : IRngProvider
{
    private Random _random = new();

    public int CurrentSeed { get; private set; } = Environment.TickCount;

    public void Reseed(int seed)
    {
        CurrentSeed = seed;
        _random = new Random(seed);
    }

    public float NextFloat()
    {
        double value = _random.NextDouble();
        return (float)value;
    }
}
