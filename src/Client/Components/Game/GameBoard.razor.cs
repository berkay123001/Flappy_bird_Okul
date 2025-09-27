using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Client.Models;
using Client.Services;
using Client.Services.Physics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Components.Game;

public sealed partial class GameBoard : ComponentBase, IAsyncDisposable
{
    private const float BirdX = 128f;
    private const float BirdHalfWidth = 17f;
    private const float BirdHalfHeight = 12f;
    private const float PipeWidth = 80f;
    private const float GameHeight = PhysicsEngine.GroundLevel;
    private const float PipeFloor = PhysicsEngine.GroundLevel + BirdHalfHeight;

    [SuppressMessage("Style", "IDE0052", Justification = "Bound in Razor markup for asset preloading.")]
    private static readonly string[] PreloadAssets =
    [
        "assets/sprites/background-day.png",
        "assets/sprites/base.png",
        "assets/sprites/yellowbird-upflap.png",
        "assets/sprites/yellowbird-midflap.png",
        "assets/sprites/yellowbird-downflap.png",
        "assets/sprites/pipe-green.png",
        "assets/sprites/gameover.png",
        "assets/sprites/message.png",
    ];

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private CancellationTokenSource? _lifetimeCts;
    private bool _hasStarted;
    [SuppressMessage("Style", "IDE0052", Justification = "Updated via Razor focus bindings.")]
    private bool _isFocused;

    private bool _isDisposed;
    private bool _focusRequested;

    [SuppressMessage("Style", "IDE0052", Justification = "Consumed by HUD binding in Razor view.")]
    private int _score;

    [SuppressMessage("Style", "IDE0052", Justification = "Consumed by HUD binding in Razor view.")]
    private int _highScore;

    private ElementReference BoardElement { get; set; }

    [Inject]
    public GameLoopService GameLoop { get; set; } = default!;

    [Inject]
    public InputHandlerService InputHandler { get; set; } = default!;

    [Inject]
    public ScoreService ScoreService { get; set; } = default!;

    [Inject]
    public AudioService AudioService { get; set; } = default!;

    private GameState GameState => GameLoop.State;

    [SuppressMessage("Style", "IDE0051", Justification = "Evaluated within Razor markup.")]
    private bool IsGameOver => _hasStarted && !GameLoop.IsRunning && !GameState.Bird.IsAlive;

    [SuppressMessage("Style", "IDE0051", Justification = "Evaluated within Razor markup.")]
    private bool ShowIntroOverlay => !_hasStarted && !GameLoop.IsRunning;

    [SuppressMessage("Style", "IDE0051", Justification = "Evaluated within Razor markup.")]
    private string BirdStyle
    {
        get
        {
            float clampedY = Math.Clamp(GameState.Bird.PositionY, 0f, GameHeight);
            float top = clampedY - BirdHalfHeight;
            float left = BirdX - BirdHalfWidth;
            float velocity = GameState.Bird.VelocityY;
            float rotation = Math.Clamp(-velocity / 300f, -0.6f, 0.6f) * 45f;

            return string.Create(
                CultureInfo.InvariantCulture,
                $"left: {left:F1}px; top: {top:F1}px; transform: rotate({rotation:F1}deg);");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _lifetimeCts = new CancellationTokenSource();

        SubscribeEvents();

        try
        {
            await GameLoop.InitializeAsync(_lifetimeCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_lifetimeCts.IsCancellationRequested)
        {
            return;
        }

        _ = PreloadAssets.Length;

        _score = ScoreService.CurrentScore;
        _highScore = ScoreService.HighScore;

        if (!GameLoop.IsRunning)
        {
            GameState.Reset(PhysicsEngine.GroundLevel * 0.5f);
            GameState.Pause();
        }

        _focusRequested = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if ((_focusRequested || firstRender) && !_isDisposed)
        {
            _focusRequested = false;
            await FocusBoardInternalAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        UnsubscribeEvents();

        if (_lifetimeCts is not null)
        {
            try
            {
                _lifetimeCts.Cancel();
                _lifetimeCts.Dispose();
            }
            catch
            {
                // Best effort
            }
        }

        try
        {
            await GameLoop.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Swallow during disposal.
        }
    }

    private void SubscribeEvents()
    {
        InputHandler.InputAccepted += OnInputAccepted;
        GameLoop.StateChanged += OnGameStateChanged;
        GameLoop.GameStarted += OnGameStarted;
        GameLoop.GameOver += OnGameOver;
        ScoreService.ScoreChanged += OnScoreChanged;
    }

    private void UnsubscribeEvents()
    {
        InputHandler.InputAccepted -= OnInputAccepted;
        GameLoop.StateChanged -= OnGameStateChanged;
        GameLoop.GameStarted -= OnGameStarted;
        GameLoop.GameOver -= OnGameOver;
        ScoreService.ScoreChanged -= OnScoreChanged;
    }

    private async Task StartGameAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await FocusBoardAsync().ConfigureAwait(false);
        await EnsureGameLoopRunningAsync().ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via HUD event binding.")]
    private async Task RestartGameAsync()
    {
        _hasStarted = false;
        await StartGameAsync().ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via Razor pointer binding.")]
    private async Task OnPointerDownAsync(PointerEventArgs args)
    {
        if (_isDisposed)
        {
            return;
        }

        await FocusBoardAsync().ConfigureAwait(false);

        InputType inputType = args.PointerType?.ToLowerInvariant() switch
        {
            "touch" => InputType.TouchTap,
            "pen" => InputType.TouchTap,
            _ => InputType.MouseClick,
        };

        await SubmitInputAsync(inputType).ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via Razor keyboard binding.")]
    private async Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!IsSpaceKey(args))
        {
            return;
        }

        await SubmitInputAsync(InputType.SpaceKey).ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via Razor focus binding.")]
    private Task OnBoardFocused(FocusEventArgs args)
    {
        _ = args;
        _isFocused = true;
        return Task.CompletedTask;
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via Razor focus binding.")]
    private Task OnBoardBlurred(FocusEventArgs args)
    {
        _ = args;
        _isFocused = false;
        return Task.CompletedTask;
    }

    private async Task SubmitInputAsync(InputType inputType)
    {
        if (_isDisposed)
        {
            return;
        }

        await EnsureGameLoopRunningAsync().ConfigureAwait(false);

        double timestamp = _stopwatch.Elapsed.TotalMilliseconds;

        try
        {
            bool _ = await InputHandler.HandleInputAsync(inputType, timestamp, _lifetimeCts?.Token ?? CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore when shutting down.
        }
    }

    private async Task EnsureGameLoopRunningAsync()
    {
        if (GameLoop.IsRunning)
        {
            return;
        }

        try
        {
            await GameLoop.StartAsync(_lifetimeCts?.Token ?? CancellationToken.None).ConfigureAwait(false);
            _hasStarted = true;
            _focusRequested = true;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task FocusBoardAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _focusRequested = true;
        await FocusBoardInternalAsync().ConfigureAwait(false);
    }

    private async Task FocusBoardInternalAsync()
    {
        try
        {
            await BoardElement.FocusAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore focus failures.
        }
    }

    private void OnInputAccepted(InputType inputType, double timestamp)
    {
        _ = inputType;
        _ = timestamp;
        GameLoop.EnqueueInput(inputType, timestamp);
    }

    private void OnGameStateChanged(GameState state)
    {
        _ = state;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnGameStarted()
    {
        _ = InvokeAsync(async () =>
        {
            if (_isDisposed)
            {
                return;
            }

            _hasStarted = true;
            await FocusBoardInternalAsync().ConfigureAwait(false);
            StateHasChanged();
        });
    }

    private void OnGameOver() => _ = InvokeAsync(StateHasChanged);

    private void OnScoreChanged(int score, int highScore)
    {
        _score = score;
        _highScore = highScore;
        _ = InvokeAsync(StateHasChanged);
    }

    private static bool IsSpaceKey(KeyboardEventArgs args)
    {
        string? key = args.Key;
        if (!string.IsNullOrEmpty(key) && (key == " " || key.Equals("Space", StringComparison.OrdinalIgnoreCase) || key.Equals("Spacebar", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string? code = args.Code;
        return !string.IsNullOrEmpty(code) && (code.Equals("Space", StringComparison.OrdinalIgnoreCase) || code.Equals("Spacebar", StringComparison.OrdinalIgnoreCase));
    }

    [SuppressMessage("Style", "IDE0051", Justification = "Invoked via Razor pipe markup.")]
    private string GetPipeStyle(PipePair pipe, bool isTop)
    {
        float left = pipe.PositionX;
        float height;
        float top = 0f;

        if (isTop)
        {
            height = Math.Max(0f, pipe.GapTop);
        }
        else
        {
            top = pipe.GapBottom;
            height = Math.Max(0f, PipeFloor - pipe.GapBottom);
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"left: {left:F1}px; top: {top:F1}px; width: {PipeWidth:F1}px; height: {height:F1}px;");
    }
}
