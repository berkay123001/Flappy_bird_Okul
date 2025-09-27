namespace Client.Integration.GameFlow;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GameFlowCollection : ICollectionFixture<GameFlowFixture>
{
    public const string Name = "Game Flow";
}
