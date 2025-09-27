using System.Reflection;

namespace Client.Interop;

public sealed class StorageInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<StorageInterop> _logger;
    private readonly string _modulePath;
    private readonly SemaphoreSlim _moduleLock = new(1, 1);
    private IJSObjectReference? _module;

    public StorageInterop(IJSRuntime jsRuntime, ILogger<StorageInterop> logger, string modulePath = "./js/storage.js")
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(modulePath);

        _jsRuntime = jsRuntime;
        _logger = logger;
        _modulePath = modulePath;
    }

    public async ValueTask<int> GetNumberAsync(string key, int defaultValue, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [key, defaultValue];
            return await module.InvokeAsync<int>("getNumber", args).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to read key {Key} from storage module.", key);
            throw;
        }
    }

    public async ValueTask SetNumberAsync(string key, int value, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [key, value];
            await InvokeVoidAsync(module, "setNumber", args).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to set key {Key} in storage module.", key);
            throw;
        }
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        IJSObjectReference module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            object?[] args = [key];
            await InvokeVoidAsync(module, "remove", args).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to remove key {Key} from storage module.", key);
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

        _moduleLock.Dispose();
    }

    private static async ValueTask InvokeVoidAsync(IJSObjectReference module, string identifier, object?[] args)
    {
        static MethodInfo? FindMethod(Type type)
        {
            return type.GetMethod(
                "InvokeVoidAsync",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                [typeof(string), typeof(object[])]);
        }

        MethodInfo? method = FindMethod(module.GetType());

        if (method is null)
        {
            await JSObjectReferenceExtensions.InvokeVoidAsync(module, identifier, args).ConfigureAwait(false);
            return;
        }

        MethodInfo methodInfo = method;
        object? result = methodInfo.Invoke(module, [identifier, args]);

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
