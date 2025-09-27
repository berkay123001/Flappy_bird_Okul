namespace Client.Models;

public readonly record struct ScoreSnapshot(int Score, int HighScore, DateTime Timestamp)
{
    public static ScoreSnapshot Create(int score, int highScore, DateTime timestamp) =>
        new(score, highScore, timestamp);

    public int ScoreDelta(ScoreSnapshot previous) => Score - previous.Score;

    public int HighScoreDelta(ScoreSnapshot previous) => HighScore - previous.HighScore;
}
