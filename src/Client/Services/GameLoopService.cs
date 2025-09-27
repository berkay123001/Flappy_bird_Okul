using System.Numerics;
using Client.Diagnostics;
using Client.Interop;
using Client.Models;
using Client.Services.Physics;

namespace Client.Services;

public sealed class GameLoopService : IAsyncDisposable
{
    private const float SpawnIntervalSeconds = 1.5f;
    private const float BirdSpawnY = PhysicsEngine.GroundLevel * 0.5f;
    private const float BirdX = 128f;
    private const float BirdHalfWidth = 17f;
    private const float BirdHalfHeight = 12f;
    private const float PipeWidth = 80f;
    private const float PipeLeftBoundary = -PipeWidth;

    private readonly AnimationInterop _animationInterop;
    private readonly PhysicsEngine _physicsEngine;
    private readonly ObstacleSpawner _obstacleSpawner;
    private readonly IRngProvider _rngProvider;
    private readonly ScoreService _scoreService;
    private readonly AudioService _audioService;
    private readonly DebugOverlayService _debugOverlay;
    private readonly ILogger<GameLoopService> _logger;
    private readonly InputQueue _inputQueue = new();
    private int _frameGuard;

    private bool _initialized;
    private int? _loopId;
    private float _spawnAccumulator;

    public GameLoopService(
        AnimationInterop animationInterop,
        PhysicsEngine physicsEngine,
        ObstacleSpawner obstacleSpawner,
        IRngProvider rngProvider,
        ScoreService scoreService,
        AudioService audioService,
        DebugOverlayService debugOverlay,
        ILogger<GameLoopService> logger)
    {
        ArgumentNullException.ThrowIfNull(animationInterop);
        ArgumentNullException.ThrowIfNull(physicsEngine);
        ArgumentNullException.ThrowIfNull(obstacleSpawner);
        ArgumentNullException.ThrowIfNull(rngProvider);
        ArgumentNullException.ThrowIfNull(scoreService);
        ArgumentNullException.ThrowIfNull(audioService);
        ArgumentNullException.ThrowIfNull(debugOverlay);
        ArgumentNullException.ThrowIfNull(logger);

        _animationInterop = animationInterop;
        _physicsEngine = physicsEngine;
        _obstacleSpawner = obstacleSpawner;
        _rngProvider = rngProvider;
        _scoreService = scoreService;
        _audioService = audioService;
        _debugOverlay = debugOverlay;
        _logger = logger;
    }

    public GameState State { get; } = new();

    public bool IsRunning { get; private set; }

    public event Action<GameState>? StateChanged;

    public event Action? GameStarted;

    public event Action? GameOver;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_initialized)
        {
            return;
        }

        await _scoreService.InitializeAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _audioService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "Audio initialization failed; continuing without preloading.");
        }

        State.UpdateHighScore(_scoreService.HighScore);
        _initialized = true;
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        if (IsRunning)
        {
            await StopAsync(cancellationToken).ConfigureAwait(false);
        }

        await _scoreService.ResetAsync(cancellationToken).ConfigureAwait(false);
        State.UpdateHighScore(_scoreService.HighScore);

        try
        {
            await _audioService.StopAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to stop audio during game start.");
        }

        _inputQueue.Clear();
        _debugOverlay.Reset();
        _spawnAccumulator = 0f;

        int seed = Environment.TickCount;
        _rngProvider.Reseed(seed);
        State.Initialize(seed, _scoreService.HighScore, BirdSpawnY);

        SpawnPipe();

        int loopId = await _animationInterop.StartLoopAsync(OnAnimationFrameAsync, cancellationToken).ConfigureAwait(false);
        _loopId = loopId;
        IsRunning = true;

        try
        {
            GameStarted?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GameStarted callback failed.");
        }

        OnStateChanged();
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRunning)
        {
            return;
        }

        await StopLoopInternalAsync().ConfigureAwait(false);
        State.Pause();
        _inputQueue.Clear();

        try
        {
            await _audioService.StopAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger.LogDebug(ex, "Failed to stop audio during loop stop.");
        }

        OnStateChanged();
    }

    public void EnqueueInput(InputType inputType, double timestamp)
    {
        if (!IsRunning || !State.Bird.IsAlive)
        {
            return;
        }

        _inputQueue.Enqueue(inputType, timestamp);
    }

    private async ValueTask OnAnimationFrameAsync(double deltaMilliseconds)
    {
        if (Interlocked.CompareExchange(ref _frameGuard, 1, 0) != 0)
        {
            return;
        }

        try
        {
            if (!IsRunning)
            {
                return;
            }

            if (double.IsNaN(deltaMilliseconds) || double.IsInfinity(deltaMilliseconds) || deltaMilliseconds <= 0d)
            {
                return;
            }

            _debugOverlay.RecordFrame(TimeSpan.FromMilliseconds(deltaMilliseconds));
            State.ApplyFrameDelta((float)deltaMilliseconds);

            int processedInputs = ProcessInputQueue();

            float deltaSeconds = (float)(deltaMilliseconds / 1000d);
            AdvanceBird(deltaSeconds);

            bool collisionDetected = false;
            Vector2 collisionPoint = default;

            List<PipePair> obstacles = State.Obstacles;
            for (int i = 0; i < obstacles.Count; i++)
            {
                PipePair pipe = obstacles[i].Advance(deltaSeconds);

                if (!collisionDetected && DetectCollision(State.Bird, pipe, out collisionPoint))
                {
                    collisionDetected = true;
                }

                if (!pipe.Passed && pipe.PositionX + PipeWidth < BirdX)
                {
                    pipe = pipe.MarkPassed();
                    obstacles[i] = pipe;
                    await HandlePipePassedAsync().ConfigureAwait(false);
                }
                else
                {
                    obstacles[i] = pipe;
                }

                if (pipe.IsOutOfBounds(PipeLeftBoundary))
                {
                    obstacles.RemoveAt(i--);
                    continue;
                }
            }

            _spawnAccumulator += deltaSeconds;
            while (_spawnAccumulator >= SpawnIntervalSeconds)
            {
                SpawnPipe();
                _spawnAccumulator -= SpawnIntervalSeconds;
            }

            if (!collisionDetected && !State.Bird.IsAlive)
            {
                collisionDetected = true;
                collisionPoint = new Vector2(BirdX, State.Bird.PositionY);
            }

            if (collisionDetected)
            {
                await HandleGameOverAsync(collisionPoint).ConfigureAwait(false);
                return;
            }

            if (processedInputs > 0)
            {
                await PlayFlapAsync().ConfigureAwait(false);
            }

            OnStateChanged();
        }
        finally
        {
            Volatile.Write(ref _frameGuard, 0);
        }
    }

    private int ProcessInputQueue()
    {
        if (!State.Bird.IsAlive)
        {
            _inputQueue.Clear();
            return 0;
        }

        int processed = 0;
        while (_inputQueue.TryDequeue(out InputQueue.InputEvent input))
        {
            State.Bird.QueueImpulse(input.Timestamp);
            processed++;
        }

        return processed;
    }

    private void AdvanceBird(float deltaSeconds)
    {
        BirdState current = State.Bird;
        BirdState advanced = _physicsEngine.AdvanceBird(current, deltaSeconds);

        if (ReferenceEquals(current, advanced))
        {
            return;
        }

        current.PositionY = advanced.PositionY;
        current.VelocityY = advanced.VelocityY;
        current.IsAlive = advanced.IsAlive;
        current.PendingImpulse = advanced.PendingImpulse;
        current.LastInputTimestamp = advanced.LastInputTimestamp;
    }

    private bool DetectCollision(BirdState bird, PipePair pipe, out Vector2 collisionPoint)
    {
        float birdLeft = BirdX - BirdHalfWidth;
        float birdRight = BirdX + BirdHalfWidth;
        float birdTop = bird.PositionY - BirdHalfHeight;
        float birdBottom = bird.PositionY + BirdHalfHeight;

        float pipeLeft = pipe.PositionX;
        float pipeRight = pipe.PositionX + PipeWidth;

        bool horizontalOverlap = birdRight >= pipeLeft && birdLeft <= pipeRight;
        bool outsideGap = birdTop <= pipe.GapTop || birdBottom >= pipe.GapBottom;

        if (horizontalOverlap && outsideGap)
        {
            collisionPoint = new Vector2(BirdX, bird.PositionY);
            return true;
        }

        collisionPoint = default;
        return false;
    }

    private async ValueTask HandlePipePassedAsync()
    {
        bool isNewHighScore = State.TryIncrementScore();

        try
        {
            await _scoreService.AddPointAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update score after passing obstacle.");
        }

        State.UpdateHighScore(_scoreService.HighScore);

        if (isNewHighScore)
        {
            _logger.LogDebug("New high score reached: {Score}", State.HighScore);
        }

        try
        {
            await _audioService.PlayAsync(AudioEffect.Score, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to play score audio.");
        }
    }

    private async ValueTask PlayFlapAsync()
    {
        try
        {
            await _audioService.PlayAsync(AudioEffect.Flap, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Failed to play flap audio.");
        }
    }

    private async ValueTask HandleGameOverAsync(Vector2 collisionPoint)
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        State.Pause();
        State.Bird.Kill();
        _inputQueue.Clear();
        _debugOverlay.RecordCollision(collisionPoint, DateTimeOffset.UtcNow);

        await StopLoopInternalAsync().ConfigureAwait(false);

        try
        {
            await _audioService.PlayAsync(AudioEffect.Hit, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Failed to play collision audio.");
        }

        try
        {
            GameOver?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GameOver callback failed.");
        }

        OnStateChanged();
    }

    private void SpawnPipe()
    {
        PipePair pipe = _obstacleSpawner.SpawnNext();
        State.Obstacles.Add(pipe);
    }

    private async ValueTask StopLoopInternalAsync()
    {
        int? loopId = _loopId;
        _loopId = null;
        IsRunning = false;

        if (loopId is int id)
        {
            try
            {
                await _animationInterop.StopLoopAsync(id, CancellationToken.None).ConfigureAwait(false);
            }
            catch (JSException ex)
            {
                _logger.LogDebug(ex, "Failed to stop animation loop {LoopId}.", id);
            }
        }
    }

    private void OnStateChanged()
    {
        Action<GameState>? handler = StateChanged;
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StateChanged callback failed.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopLoopInternalAsync().ConfigureAwait(false);
        _inputQueue.Clear();
    }
}
