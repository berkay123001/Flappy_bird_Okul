using System.Diagnostics.CodeAnalysis;
using Client.Interop;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace Client.Tests.Interop;

public sealed class AudioInteropTests
{
    private const string ModulePath = "./js/audio.js";

    [Fact]
    public async Task PreloadSetsAllFilesAsync()
    {
        (AudioInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        AudioManifest manifest = new("wwwroot/assets", new Dictionary<string, string>
        {
            ["flap"] = "flap.mp3",
            ["score"] = "score.mp3",
            ["hit"] = "hit.mp3",
        });

        await sut.PreloadAsync(manifest, CancellationToken.None);

        Assert.Equal("preloadAssets", module.LastVoidIdentifier);
        Assert.NotNull(module.LastVoidArgs);
        Assert.Single(module.LastVoidArgs!, manifest);
    }

    [Fact]
    public async Task PlayEffectRoutesEnumAsync()
    {
        (AudioInterop sut, RecordingJsModule module) = CreateSystemUnderTest();

        await sut.PlayEffectAsync("score", CancellationToken.None);

        Assert.Equal("playEffect", module.LastVoidIdentifier);
        Assert.NotNull(module.LastVoidArgs);
        Assert.Single(module.LastVoidArgs!, "score");
    }

    [Fact]
    public async Task MissingAssetRaisesErrorAsync()
    {
        (AudioInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.ThrowOnPlay = true;

        JSException exception = await Assert.ThrowsAsync<JSException>(() => sut.PlayEffectAsync("hit", CancellationToken.None).AsTask());
        Assert.Equal("AudioMissingError", exception.Message);
    }

    private static (AudioInterop sut, RecordingJsModule module) CreateSystemUnderTest()
    {
        RecordingJsModule module = new();
        RecordingJsRuntime jsRuntime = new(module);
        AudioInterop sut = new(jsRuntime, NullLogger<AudioInterop>.Instance, ModulePath);
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
        public string? LastVoidIdentifier { get; private set; }

        public object?[]? LastVoidArgs { get; private set; }

        public bool ThrowOnPlay { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            throw new NotSupportedException($"InvokeAsync for '{identifier}' is not supported in tests.");

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

            return ThrowOnPlay && identifier == "playEffect"
                ? ValueTask.FromException(new JSException("AudioMissingError"))
                : ValueTask.CompletedTask;
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
