using System.Reflection;

namespace Client.Interop;

public sealed record AudioManifest(string Root, IReadOnlyDictionary<string, string> Files);

public sealed class AudioInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AudioInterop> _logger;
    private readonly string _modulePath;
    private readonly SemaphoreSlim _moduleLock = new(1, 1);
    private IJSObjectReference? _module;

    public AudioInterop(IJSRuntime jsRuntime, ILogger<AudioInterop> logger, string modulePath = "./js/audio.js")
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(modulePath);

        _jsRuntime = jsRuntime;
        _logger = logger;
        _modulePath = modulePath;
    }

    public async ValueTask PreloadAsync(AudioManifest manifest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [manifest];
            await InvokeVoidAsync(module, "preloadAssets", args, cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to preload audio assets.");
            throw;
        }
    }

    public async ValueTask PlayEffectAsync(string effectName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(effectName);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [effectName];
            await InvokeVoidAsync(module, "playEffect", args, cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to play audio effect {EffectName}.", effectName);
            throw;
        }
    }

    public async ValueTask StopAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [];
            await InvokeVoidAsync(module, "stopAll", args, cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to stop audio playback.");
            throw;
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync(CancellationToken cancellationToken)
    {
        if (_module is { } cached)
        {
            return cached;
        }

        await _moduleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_module is { } existing)
            {
                return existing;
            }

            IJSObjectReference module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, [_modulePath]).ConfigureAwait(false);
            _module = module;
            return module;
        }
        finally
        {
            _ = _moduleLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is { } module)
        {
            await module.DisposeAsync().ConfigureAwait(false);
        }

        _moduleLock.Dispose();
    }

    private static async ValueTask InvokeVoidAsync(IJSObjectReference module, string identifier, object?[] args, CancellationToken cancellationToken)
    {
        static MethodInfo? FindMethod(Type type)
        {
            MethodInfo? withoutToken = type.GetMethod(
                "InvokeVoidAsync",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                [typeof(string), typeof(object[])]);

            return withoutToken ?? type.GetMethod(
                "InvokeVoidAsync",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                [typeof(string), typeof(CancellationToken), typeof(object[])]);
        }

        MethodInfo? method = FindMethod(module.GetType());

        if (method is null)
        {
            await JSObjectReferenceExtensions.InvokeVoidAsync(module, identifier, cancellationToken, args).ConfigureAwait(false);
            return;
        }

        object?[] parameters = method.GetParameters().Length switch
        {
            2 => [identifier, args],
            3 => [identifier, cancellationToken, args],
            _ => throw new InvalidOperationException("Unsupported InvokeVoidAsync signature."),
        };

        object? result = method.Invoke(module, parameters);

        if (result is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
        }
        else if (result is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }
}
