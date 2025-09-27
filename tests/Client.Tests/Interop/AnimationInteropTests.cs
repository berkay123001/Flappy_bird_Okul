using Client.Interop;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;

namespace Client.Tests.Interop;

public sealed class AnimationInteropTests
{
    private const string ModulePath = "./js/animation.js";

    [Fact]
    public async Task StartLoopRegistersCallback()
    {
        (AnimationInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.StartLoopResult = 42;

        AnimationFrameRecorder recorder = new();

        int result = await sut.StartLoopAsync(recorder.OnFrameAsync, CancellationToken.None);

        Assert.Equal(42, result);
        Assert.Equal("startLoop", module.LastInvokeIdentifier);
        object?[]? args = module.LastInvokeArgs;
        Assert.NotNull(args);
        Assert.Equal(2, args!.Length);
        Assert.Equal(nameof(AnimationInterop.OnAnimationFrameAsync), args[1]);
    }

    [Fact]
    public async Task StopLoopCancelsRequest()
    {
        (AnimationInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.StartLoopResult = 7;

        AnimationFrameRecorder recorder = new();
        int loopId = await sut.StartLoopAsync(recorder.OnFrameAsync, CancellationToken.None);

        await sut.StopLoopAsync(loopId, CancellationToken.None);

        Assert.Equal("stopLoop", module.LastVoidIdentifier);
        object?[]? args = module.LastVoidArgs;
        Assert.NotNull(args);
        Assert.Single(args!, loopId);
    }

    [Fact]
    public async Task DeltaComputationIsPositive()
    {
        (AnimationInterop sut, RecordingJsModule module) = CreateSystemUnderTest();
        module.StartLoopResult = 11;

        AnimationFrameRecorder recorder = new();
        _ = await sut.StartLoopAsync(recorder.OnFrameAsync, CancellationToken.None);

        await sut.OnAnimationFrameAsync(1000);
        await sut.OnAnimationFrameAsync(1016.67);

        double delta = Assert.Single(recorder.Deltas);
        Assert.True(delta > 0);
    }

    private static (AnimationInterop sut, RecordingJsModule module) CreateSystemUnderTest()
    {
        RecordingJsModule module = new();
        Mock<IJSRuntime> jsRuntimeMock = new(MockBehavior.Strict);

        _ = jsRuntimeMock
            .Setup(js => js.InvokeAsync<IJSObjectReference>("import", It.Is<object?[]>(args => args != null && args.Length == 1 && string.Equals(args[0] as string, ModulePath, StringComparison.Ordinal))))
            .ReturnsAsync(module);

        AnimationInterop sut = new(jsRuntimeMock.Object, NullLogger<AnimationInterop>.Instance, ModulePath);
        return (sut, module);
    }

    private sealed class AnimationFrameRecorder
    {
        private readonly List<double> _deltas = [];

        public IReadOnlyList<double> Deltas => _deltas;

        public ValueTask OnFrameAsync(double delta)
        {
            _deltas.Add(delta);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingJsModule : IJSObjectReference
    {
        public int StartLoopResult { get; set; }

        public string? LastInvokeIdentifier { get; private set; }

        public object?[]? LastInvokeArgs { get; private set; }

        public string? LastVoidIdentifier { get; private set; }

        public object?[]? LastVoidArgs { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            LastInvokeIdentifier = identifier;
            LastInvokeArgs = args;

            return identifier is "startLoop" && typeof(TValue) == typeof(int)
                ? (ValueTask<TValue>)(object)new ValueTask<int>(StartLoopResult)
                : throw new NotSupportedException($"InvokeAsync for '{identifier}' with return type '{typeof(TValue).Name}' is not supported in tests.");
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            _ = cancellationToken;
            return InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
        {
            LastVoidIdentifier = identifier;
            LastVoidArgs = args;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
