using System.Reflection;

namespace Client.Interop;

public sealed class AnimationInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AnimationInterop> _logger;
    private readonly string _modulePath;
    private readonly SemaphoreSlim _moduleLock = new(1, 1);

    private IJSObjectReference? _module;
    private DotNetObjectReference<AnimationInterop>? _selfReference;
    private Func<double, ValueTask>? _onFrame;
    private double _lastTimestamp = double.NaN;

    public AnimationInterop(IJSRuntime jsRuntime, ILogger<AnimationInterop> logger, string modulePath = "./js/animation.js")
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(modulePath);

        _jsRuntime = jsRuntime;
        _logger = logger;
        _modulePath = modulePath;
    }

    public async ValueTask<int> StartLoopAsync(Func<double, ValueTask> onFrame, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onFrame);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        _onFrame = onFrame;
        _lastTimestamp = double.NaN;
        DotNetObjectReference<AnimationInterop> selfReference = _selfReference ??= DotNetObjectReference.Create(this);

        object?[] args = [selfReference, nameof(OnAnimationFrameAsync)];

        try
        {
            return await module.InvokeAsync<int>("startLoop", args).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Failed to start animation loop.");
            throw;
        }
    }

    public async ValueTask StopLoopAsync(int loopId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        object?[] args = [loopId];

        try
        {
            await InvokeVoidAsync(module, "stopLoop", args, cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to stop animation loop {LoopId}.", loopId);
            throw;
        }
    }

    [JSInvokable]
    public async ValueTask OnAnimationFrameAsync(double timestamp)
    {
        Func<double, ValueTask>? callback = _onFrame;

        if (callback is null)
        {
            _logger.LogDebug("Animation frame received without active callback.");
            return;
        }

        double previous = _lastTimestamp;
        _lastTimestamp = timestamp;

        if (double.IsNaN(previous) || timestamp <= previous)
        {
            return;
        }

        double delta = timestamp - previous;

        try
        {
            await callback(delta).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Animation frame callback failed.");
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

            cancellationToken.ThrowIfCancellationRequested();
            IJSObjectReference module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", [_modulePath]).ConfigureAwait(false);
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

        _selfReference?.Dispose();
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
