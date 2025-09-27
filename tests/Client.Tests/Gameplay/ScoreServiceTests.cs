using Client.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Client.Tests.Gameplay;

public sealed class ScoreServiceTests
{
    private const int ExistingHighScore = 10;

    [Fact]
    public async Task InitializeLoadsHighScoreAsync()
    {
        var storage = RecordingStorageService.WithHighScore(ExistingHighScore);
        ScoreService service = CreateService(storage);
        List<(int score, int highScore)> notifications = Hook(service);

        await service.InitializeAsync(CancellationToken.None);

        Assert.Equal(0, service.CurrentScore);
        Assert.Equal(ExistingHighScore, service.HighScore);
        Assert.Collection(notifications, item => Assert.Equal((0, ExistingHighScore), item));
    }

    [Fact]
    public async Task AddPointIncrementsScoreAndPersistsHighScoreAsync()
    {
        var storage = RecordingStorageService.WithHighScore(0);
        ScoreService service = CreateService(storage);
        List<(int score, int highScore)> notifications = Hook(service);
        await service.InitializeAsync(CancellationToken.None);
        notifications.Clear();

        await service.AddPointAsync(CancellationToken.None);

        Assert.Equal(1, service.CurrentScore);
        Assert.Equal(1, service.HighScore);
        Assert.Equal(1, storage.PersistCount);
        Assert.Collection(notifications, item => Assert.Equal((1, 1), item));
    }

    [Fact]
    public async Task AddPointBelowHighScoreDoesNotPersistAsync()
    {
        var storage = RecordingStorageService.WithHighScore(ExistingHighScore);
        ScoreService service = CreateService(storage);
        await service.InitializeAsync(CancellationToken.None);

        await service.AddPointAsync(CancellationToken.None);

        Assert.Equal(1, service.CurrentScore);
        Assert.Equal(ExistingHighScore, service.HighScore);
        Assert.Equal(0, storage.PersistCount);
    }

    [Fact]
    public async Task ResetClearsScoreAndNotifiesAsync()
    {
        var storage = RecordingStorageService.WithHighScore(ExistingHighScore);
        ScoreService service = CreateService(storage);
        List<(int score, int highScore)> notifications = Hook(service);
        await service.InitializeAsync(CancellationToken.None);
        await service.AddPointAsync(CancellationToken.None);
        notifications.Clear();

        await service.ResetAsync(CancellationToken.None);

        Assert.Equal(0, service.CurrentScore);
        Assert.Equal(ExistingHighScore, service.HighScore);
        Assert.Collection(notifications, item => Assert.Equal((0, ExistingHighScore), item));
    }

    private static ScoreService CreateService(IStorageService storage) =>
        new(storage, NullLogger<ScoreService>.Instance);

    private static List<(int score, int highScore)> Hook(ScoreService service)
    {
        var notifications = new List<(int score, int highScore)>();
        service.ScoreChanged += (score, highScore) => notifications.Add((score, highScore));
        return notifications;
    }

    private sealed class RecordingStorageService : IStorageService
    {
        private RecordingStorageService(int highScore)
        {
            NextHighScore = highScore;
        }

        public int NextHighScore { get; set; }

        public int PersistCount { get; private set; }

        public int LastPersistedScore { get; private set; }

        public static RecordingStorageService WithHighScore(int highScore) => new(highScore);

        public ValueTask<int> GetHighScoreAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<int>(NextHighScore);
        }

        public ValueTask SetHighScoreAsync(int highScore, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PersistCount++;
            LastPersistedScore = highScore;
            NextHighScore = highScore;
            return ValueTask.CompletedTask;
        }
    }
}
