using Microsoft.AspNetCore.Components;

namespace Client.Components.Hud;

public sealed partial class HudPanel : ComponentBase
{
    [Parameter]
    public int Score { get; set; }

    [Parameter]
    public int HighScore { get; set; }

    [Parameter]
    public bool IsRunning { get; set; }

    [Parameter]
    public bool IsGameOver { get; set; }

    [Parameter]
    public string AudioPrompt { get; set; } = string.Empty;

    [Parameter]
    public EventCallback StartRequested { get; set; }

    [Parameter]
    public EventCallback RestartRequested { get; set; }

}
