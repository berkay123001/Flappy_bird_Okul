using Client.Interop;

namespace Client.Services;

public enum AudioEffect
{
    Flap,
    Score,
    Hit,
}

public sealed class AudioService
{
    private const string AutoplayBlockedError = "AudioAutoplayBlocked";
    private const string MissingAssetError = "AudioMissingError";
    private const int DuplicateSuppressWindowMilliseconds = 16;

    private static readonly IReadOnlyDictionary<AudioEffect, string> EffectNames = new Dictionary<AudioEffect, string>
    {
        [AudioEffect.Flap] = "flap",
        [AudioEffect.Score] = "score",
        [AudioEffect.Hit] = "hit",
    };

    private static readonly AudioManifest Manifest = new(
        "assets/audio",
        new Dictionary<string, string>
        {
            ["flap"] = "wing.wav",
            ["score"] = "point.wav",
            ["hit"] = "hit.wav",
        });

    private readonly AudioInterop _audioInterop;
    private readonly ILogger<AudioService> _logger;

    private string? _lastEffect;
    private long _lastEffectTimestamp;

    public AudioService(AudioInterop audioInterop, ILogger<AudioService> logger)
    {
        ArgumentNullException.ThrowIfNull(audioInterop);
        ArgumentNullException.ThrowIfNull(logger);

        _audioInterop = audioInterop;
        _logger = logger;
    }

    public bool IsInitialized { get; private set; }

    public bool IsAutoplayBlocked { get; private set; }

    public string AutoplayPrompt => IsAutoplayBlocked ? "Tıklayarak sesi etkinleştir" : string.Empty;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsInitialized)
        {
            return;
        }

        try
        {
            await _audioInterop.PreloadAsync(Manifest, cancellationToken).ConfigureAwait(false);
            IsInitialized = true;
            ResetSuppression();
        }
        catch (JSException ex) when (IsAutoplayBlockedError(ex))
        {
            HandleAutoplayBlocked(ex);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Audio assets could not be preloaded.");
            throw;
        }
    }

    public async ValueTask PlayAsync(AudioEffect effect, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string effectName = GetEffectName(effect);
        if (ShouldSuppress(effectName))
        {
            _logger.LogDebug("Skipping duplicate audio effect {Effect}", effectName);
            return;
        }

        try
        {
            await _audioInterop.PlayEffectAsync(effectName, cancellationToken).ConfigureAwait(false);
            RecordEffect(effectName);
        }
        catch (JSException ex) when (IsAutoplayBlockedError(ex))
        {
            HandleAutoplayBlocked(ex);
        }
        catch (JSException ex) when (IsMissingAssetError(ex))
        {
            _logger.LogWarning(ex, "Missing audio asset for effect {Effect}", effectName);
            throw;
        }
    }

    public async ValueTask StopAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await _audioInterop.StopAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to stop audio playback.");
            throw;
        }
        finally
        {
            ResetSuppression();
        }
    }

    public void ResetSuppression()
    {
        _lastEffect = null;
        _lastEffectTimestamp = 0;
    }

    private bool ShouldSuppress(string effectName)
    {
        long now = Environment.TickCount64;
        return string.Equals(_lastEffect, effectName, StringComparison.Ordinal) &&
               now - _lastEffectTimestamp < DuplicateSuppressWindowMilliseconds;
    }

    private void RecordEffect(string effectName)
    {
        _lastEffect = effectName;
        _lastEffectTimestamp = Environment.TickCount64;
    }

    private static string GetEffectName(AudioEffect effect) =>
        EffectNames.TryGetValue(effect, out string? name)
            ? name
            : throw new ArgumentOutOfRangeException(nameof(effect), effect, "Unsupported audio effect.");

    private static bool IsAutoplayBlockedError(JSException exception) =>
        string.Equals(exception.Message, AutoplayBlockedError, StringComparison.Ordinal);

    private static bool IsMissingAssetError(JSException exception) =>
        string.Equals(exception.Message, MissingAssetError, StringComparison.Ordinal);

    private void HandleAutoplayBlocked(JSException exception)
    {
        if (IsAutoplayBlocked)
        {
            return;
        }

        IsAutoplayBlocked = true;
        _logger.LogWarning(exception, "Browser blocked autoplay. Prompting user to enable sound.");
    }
}
