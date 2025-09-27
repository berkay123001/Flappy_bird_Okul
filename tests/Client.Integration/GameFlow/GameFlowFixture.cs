namespace Client.Integration.GameFlow;

public sealed class GameFlowFixture
{
    public Task<GameFlowDriver> CreateDriverAsync(CancellationToken cancellationToken) =>
        GameFlowDriver.CreateAsync(cancellationToken);
}
