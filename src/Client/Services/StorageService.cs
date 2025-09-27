using Client.Interop;

namespace Client.Services;

public sealed class StorageService : IStorageService
{
    private const string HighScoreKey = "flappyBird.highScore";
    private const string StorageUnavailableError = "StorageUnavailableError";

    private readonly StorageInterop _storageInterop;
    private readonly ILogger<StorageService> _logger;

    private int _fallbackHighScore;
    private bool _fallbackMode;

    public StorageService(StorageInterop storageInterop, ILogger<StorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(storageInterop);
        ArgumentNullException.ThrowIfNull(logger);

        _storageInterop = storageInterop;
        _logger = logger;
    }

    public async ValueTask<int> GetHighScoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_fallbackMode)
        {
            return _fallbackHighScore;
        }

        try
        {
            int storedValue = await _storageInterop.GetNumberAsync(HighScoreKey, _fallbackHighScore, cancellationToken).ConfigureAwait(false);
            if (storedValue < 0)
            {
                storedValue = 0;
                await PersistAsync(storedValue, force: true, cancellationToken).ConfigureAwait(false);
            }

            _fallbackHighScore = storedValue;
            return storedValue;
        }
        catch (JSException ex) when (IsStorageUnavailable(ex))
        {
            EnterFallback(ex);
            return _fallbackHighScore;
        }
    }

    public ValueTask SetHighScoreAsync(int highScore, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int normalized = Math.Max(0, highScore);
        return PersistAsync(normalized, force: false, cancellationToken);
    }

    private async ValueTask PersistAsync(int value, bool force, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!force && value <= _fallbackHighScore)
        {
            return;
        }

        if (_fallbackMode)
        {
            _fallbackHighScore = value;
            return;
        }

        try
        {
            await _storageInterop.SetNumberAsync(HighScoreKey, value, cancellationToken).ConfigureAwait(false);
            _fallbackHighScore = value;
        }
        catch (JSException ex) when (IsStorageUnavailable(ex))
        {
            EnterFallback(ex);
            _fallbackHighScore = value;
        }
    }

    private static bool IsStorageUnavailable(JSException exception) =>
        string.Equals(exception.Message, StorageUnavailableError, StringComparison.Ordinal);

    private void EnterFallback(JSException exception)
    {
        if (_fallbackMode)
        {
            return;
        }

        _fallbackMode = true;
        _logger.LogWarning(exception, "Local storage unavailable. Falling back to in-memory high score storage.");
    }
}
