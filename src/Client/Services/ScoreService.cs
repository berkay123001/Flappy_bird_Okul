namespace Client.Services;

public sealed class ScoreService
{
    private readonly IStorageService _storageService;
    private readonly ILogger<ScoreService> _logger;
    private bool _initialized;

    public ScoreService(IStorageService storageService, ILogger<ScoreService> logger)
    {
        ArgumentNullException.ThrowIfNull(storageService);
        ArgumentNullException.ThrowIfNull(logger);

        _storageService = storageService;
        _logger = logger;
    }

    public int CurrentScore { get; private set; }

    public int HighScore { get; private set; }

    public event Action<int, int>? ScoreChanged;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_initialized)
        {
            NotifyScoreChanged();
            return;
        }

        try
        {
            int storedHighScore = await _storageService.GetHighScoreAsync(cancellationToken).ConfigureAwait(false);
            HighScore = Math.Max(0, storedHighScore);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load high score from storage. Falling back to zero.");
            HighScore = 0;
        }

        CurrentScore = 0;
        _initialized = true;
        NotifyScoreChanged();
    }

    public async ValueTask AddPointAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureInitialized();

        int newScore = checked(CurrentScore + 1);
        CurrentScore = newScore;

        bool isNewHighScore = newScore > HighScore;
        if (isNewHighScore)
        {
            HighScore = newScore;

            try
            {
                await _storageService.SetHighScoreAsync(HighScore, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist high score {HighScore}.", HighScore);
            }
        }

        NotifyScoreChanged();
    }

    public ValueTask ResetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureInitialized();

        CurrentScore = 0;
        NotifyScoreChanged();
        return ValueTask.CompletedTask;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        throw new InvalidOperationException("ScoreService.InitializeAsync must be called before using the service.");
    }

    private void NotifyScoreChanged() => ScoreChanged?.Invoke(CurrentScore, HighScore);
}
