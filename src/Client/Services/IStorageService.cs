namespace Client.Services;

public interface IStorageService
{
    ValueTask<int> GetHighScoreAsync(CancellationToken cancellationToken);

    ValueTask SetHighScoreAsync(int highScore, CancellationToken cancellationToken);
}
