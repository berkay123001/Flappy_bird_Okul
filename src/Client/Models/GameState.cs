namespace Client.Models;

public sealed class GameState
{
    public BirdState Bird { get; } = new();

    public List<PipePair> Obstacles { get; } = [];

    public int Score { get; private set; }

    public int HighScore { get; private set; }

    public bool IsPaused { get; private set; }

    public float LastFrameDelta { get; private set; }

    public int Seed { get; private set; }

    public void Initialize(int seed, int highScore, float spawnY)
    {
        Seed = seed;
        HighScore = Math.Max(0, highScore);
        Reset(spawnY);
        HighScore = Math.Max(HighScore, Score);
    }

    public void Reset(float spawnY)
    {
        Score = 0;
        LastFrameDelta = 0f;
        IsPaused = false;
        Obstacles.Clear();
        Bird.Reset(spawnY);
    }

    public bool TryIncrementScore()
    {
        Score++;
        if (Score > HighScore)
        {
            HighScore = Score;
            return true;
        }

        return false;
    }

    public void UpdateHighScore(int highScore) => HighScore = Math.Max(HighScore, highScore);

    public void ApplyFrameDelta(float deltaMilliseconds) => LastFrameDelta = deltaMilliseconds;

    public void Pause() => IsPaused = true;

    public void Resume() => IsPaused = false;
}
