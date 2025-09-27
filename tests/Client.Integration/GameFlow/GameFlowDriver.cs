namespace Client.Integration.GameFlow;

public sealed class GameFlowDriver : IAsyncDisposable
{
    private GameFlowDriver()
    {
    }

    public static Task<GameFlowDriver> CreateAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public ValueTask DisposeAsync() =>
        throw NotImplemented();

    public Task GoToGameAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task WaitForBirdVisibleAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task ExpectScoreAsync(int expectedScore, CancellationToken cancellationToken) =>
        throw NotImplemented(expectedScore, cancellationToken);

    public Task ExpectHighScoreAtLeastAsync(int minimumHighScore, CancellationToken cancellationToken) =>
        throw NotImplemented(minimumHighScore, cancellationToken);

    public Task TriggerJumpAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task WaitForScoreAsync(int expectedScore, TimeSpan timeout, CancellationToken cancellationToken) =>
        throw NotImplemented(expectedScore, timeout, cancellationToken);

    public Task ForceCollisionAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task WaitForGameOverAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task ExpectGameLoopFrozenAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    public Task RestartAsync(CancellationToken cancellationToken) =>
        throw NotImplemented(cancellationToken);

    private static NotImplementedException NotImplemented(params object?[] args)
    {
        foreach (object? arg in args)
        {
            GC.KeepAlive(arg);
        }

        return new NotImplementedException();
    }
}
