using Client.Models;
using Client.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Client.Tests.Input;

public sealed class InputHandlerTests
{
    private const double CooldownMs = 120d;

    [Fact]
    public async Task FirstInputTriggersEventAsync()
    {
        InputHandlerService handler = CreateService();
        EventRecorder recorder = Hook(handler);

        bool accepted = await handler.HandleInputAsync(InputType.MouseClick, 0d, CancellationToken.None);

        Assert.True(accepted);
        Assert.Collection(recorder.Events, evt => Assert.Equal(InputType.MouseClick, evt.Type));
    }

    [Fact]
    public async Task CooldownBlocksRapidInputsAsync()
    {
        InputHandlerService handler = CreateService();
        EventRecorder recorder = Hook(handler);
        _ = await handler.HandleInputAsync(InputType.MouseClick, 0d, CancellationToken.None);

        bool accepted = await handler.HandleInputAsync(InputType.MouseClick, CooldownMs / 2d, CancellationToken.None);

        Assert.False(accepted);
        _ = Assert.Single(recorder.Events);
    }

    [Fact]
    public async Task InputsAfterCooldownAreAcceptedAsync()
    {
        InputHandlerService handler = CreateService();
        EventRecorder recorder = Hook(handler);
        _ = await handler.HandleInputAsync(InputType.MouseClick, 0d, CancellationToken.None);

        bool accepted = await handler.HandleInputAsync(InputType.SpaceKey, CooldownMs + 1d, CancellationToken.None);

        Assert.True(accepted);
        Assert.Equal(2, recorder.Events.Count);
        Assert.Equal(InputType.SpaceKey, recorder.Events.Last().Type);
    }

    [Fact]
    public async Task TouchInputNormalizesToSingleEventStreamAsync()
    {
        InputHandlerService handler = CreateService();
        EventRecorder recorder = Hook(handler);

        _ = await handler.HandleInputAsync(InputType.TouchTap, 0d, CancellationToken.None);
        bool accepted = await handler.HandleInputAsync(InputType.MouseClick, CooldownMs + 5d, CancellationToken.None);

        Assert.True(accepted);
        InputType[] expectedSequence = [InputType.TouchTap, InputType.MouseClick];
        Assert.Equal(expectedSequence, recorder.Events.Select(evt => evt.Type));
    }

    private static InputHandlerService CreateService() =>
        new(CooldownMs, NullLogger<InputHandlerService>.Instance);

    private static EventRecorder Hook(InputHandlerService handler)
    {
        EventRecorder recorder = new();
        handler.InputAccepted += recorder.Record;
        return recorder;
    }

    private sealed class EventRecorder
    {
        public List<(InputType Type, double Timestamp)> Events { get; } = [];

        public void Record(InputType type, double timestamp) => Events.Add((type, timestamp));
    }
}
