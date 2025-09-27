using System.Diagnostics.CodeAnalysis;
using Client.Interop;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace Client.Tests.Interop;

public sealed class StorageInteropTests
{
    private const string ModulePath = "./js/storage.js";
    private const string HighScoreKey = "flappyBird.highScore";

    [Fact]
    public async Task GetReturnsDefaultWhenMissingAsync()
    {
        (StorageInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.IntResult = 42;

        int result = await sut.GetNumberAsync(HighScoreKey, 42, CancellationToken.None);

        Assert.Equal("getNumber", module.LastInvokeIdentifier);
        Assert.NotNull(module.LastInvokeArgs);
        object?[] getArgs = module.LastInvokeArgs!;
        Assert.Equal(2, getArgs.Length);
        Assert.Equal(HighScoreKey, getArgs[0]);
        Assert.Equal(42, getArgs[1]);
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task SetPersistsValueAsync()
    {
        (StorageInterop sut, RecordingJsModule module) = CreateSystemUnderTest();

        await sut.SetNumberAsync(HighScoreKey, 1337, CancellationToken.None);

        Assert.Equal("setNumber", module.LastVoidIdentifier);
        Assert.NotNull(module.LastVoidArgs);
        object?[] setArgs = module.LastVoidArgs!;
        Assert.Equal(2, setArgs.Length);
        Assert.Equal(HighScoreKey, setArgs[0]);
        Assert.Equal(1337, setArgs[1]);
    }

    [Fact]
    public async Task StorageUnavailableTriggersFallbackAsync()
    {
        (StorageInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.ThrowUnavailable = true;

        JSException exception = await Assert.ThrowsAsync<JSException>(() => sut.GetNumberAsync(HighScoreKey, 0, CancellationToken.None).AsTask());
        Assert.Equal("StorageUnavailableError", exception.Message);
    }

    private static (StorageInterop sut, RecordingJsModule module) CreateSystemUnderTest()
    {
        RecordingJsModule module = new();
        RecordingJsRuntime jsRuntime = new(module);
        StorageInterop sut = new(jsRuntime, NullLogger<StorageInterop>.Instance, ModulePath);
        return (sut, module);
    }

    private sealed class RecordingJsRuntime(RecordingJsModule Module) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            identifier == "import" && typeof(TValue) == typeof(IJSObjectReference)
                ? new ValueTask<TValue>((TValue)(object)Module)
                : throw new NotSupportedException($"InvokeAsync for '{identifier}' is not supported in tests.");

        [SuppressMessage("Usage", "CA1068:CancellationToken parameters must come last", Justification = "Interface contract requires this order.")]
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return InvokeAsync<TValue>(identifier, args);
        }
    }

    private sealed class RecordingJsModule : IJSObjectReference
    {
        public string? LastInvokeIdentifier { get; private set; }

        public object?[]? LastInvokeArgs { get; private set; }

        public string? LastVoidIdentifier { get; private set; }

        public object?[]? LastVoidArgs { get; private set; }

        public int IntResult { get; set; }

        public bool ThrowUnavailable { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            LastInvokeIdentifier = identifier;
            LastInvokeArgs = args;

            return ThrowUnavailable
                ? ValueTask.FromException<TValue>(new JSException("StorageUnavailableError"))
                : identifier switch
                {
                    "getNumber" when typeof(TValue) == typeof(int) => new ValueTask<TValue>((TValue)(object)IntResult),
                    _ => throw new NotSupportedException($"InvokeAsync for '{identifier}' is not supported in tests."),
                };
        }

        [SuppressMessage("Usage", "CA1068:CancellationToken parameters must come last", Justification = "Interface contract requires this order.")]
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
        {
            LastVoidIdentifier = identifier;
            LastVoidArgs = args;

            return ThrowUnavailable
                ? ValueTask.FromException(new JSException("StorageUnavailableError"))
                : identifier switch
                {
                    "setNumber" => ValueTask.CompletedTask,
                    "remove" => ValueTask.CompletedTask,
                    _ => throw new NotSupportedException($"InvokeVoidAsync for '{identifier}' is not supported in tests."),
                };
        }

        [SuppressMessage("Usage", "CA1068:CancellationToken parameters must come last", Justification = "Interface contract requires this order.")]
        public ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return InvokeVoidAsync(identifier, args);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
