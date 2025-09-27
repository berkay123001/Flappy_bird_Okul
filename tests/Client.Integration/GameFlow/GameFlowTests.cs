namespace Client.Integration.GameFlow;

[Collection(GameFlowCollection.Name)]
public sealed class GameFlowTests(GameFlowFixture fixture)
{
    private readonly GameFlowFixture _fixture = fixture;

    [Fact]
    public async Task GameLoadsWithIdleBirdAndZeroScoreAsync()
    {
        await using GameFlowDriver driver = await _fixture.CreateDriverAsync(CancellationToken.None);

        await driver.GoToGameAsync(CancellationToken.None);
        await driver.WaitForBirdVisibleAsync(CancellationToken.None);
        await driver.ExpectScoreAsync(expectedScore: 0, CancellationToken.None);
        await driver.ExpectHighScoreAtLeastAsync(minimumHighScore: 0, CancellationToken.None);
    }

    [Fact]
    public async Task PlayerScoringUpdatesScoreboardAsync()
    {
        await using GameFlowDriver driver = await _fixture.CreateDriverAsync(CancellationToken.None);

        await driver.GoToGameAsync(CancellationToken.None);
        await driver.TriggerJumpAsync(CancellationToken.None);
        await driver.WaitForScoreAsync(expectedScore: 1, timeout: TimeSpan.FromSeconds(30), CancellationToken.None);
        await driver.ExpectHighScoreAtLeastAsync(minimumHighScore: 1, CancellationToken.None);
    }

    [Fact]
    public async Task GameOverDisplaysOverlayAndStopsInputAsync()
    {
        await using GameFlowDriver driver = await _fixture.CreateDriverAsync(CancellationToken.None);

        await driver.GoToGameAsync(CancellationToken.None);
        await driver.ForceCollisionAsync(CancellationToken.None);
        await driver.WaitForGameOverAsync(CancellationToken.None);
        await driver.TriggerJumpAsync(CancellationToken.None);
        await driver.ExpectGameLoopFrozenAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RestartResetsOnlyTransientStateAsync()
    {
        await using GameFlowDriver driver = await _fixture.CreateDriverAsync(CancellationToken.None);

        await driver.GoToGameAsync(CancellationToken.None);
        await driver.TriggerJumpAsync(CancellationToken.None);
        await driver.WaitForScoreAsync(expectedScore: 1, timeout: TimeSpan.FromSeconds(30), CancellationToken.None);
        await driver.ForceCollisionAsync(CancellationToken.None);
        await driver.WaitForGameOverAsync(CancellationToken.None);

        await driver.RestartAsync(CancellationToken.None);

        await driver.ExpectScoreAsync(expectedScore: 0, CancellationToken.None);
        await driver.ExpectHighScoreAtLeastAsync(minimumHighScore: 1, CancellationToken.None);
        await driver.WaitForBirdVisibleAsync(CancellationToken.None);
    }
}
