namespace Client.Services.Physics;

public interface IRngProvider
{
    int CurrentSeed { get; }

    void Reseed(int seed);

    float NextFloat();
}
