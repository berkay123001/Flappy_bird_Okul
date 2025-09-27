namespace Client.Models;

public sealed record class PipePair
{
    public float PositionX { get; init; }

    public float GapCenterY { get; init; }

    public float GapSize { get; init; }

    public float VelocityX { get; init; }

    public bool Passed { get; init; }

    public float HalfGap => GapSize * 0.5f;

    public float GapTop => GapCenterY - HalfGap;

    public float GapBottom => GapCenterY + HalfGap;

    public PipePair Advance(float deltaSeconds) =>
        this with { PositionX = PositionX + (VelocityX * deltaSeconds) };

    public PipePair MarkPassed() => this with { Passed = true };

    public bool IsOutOfBounds(float leftBoundary) => PositionX + VelocityX <= leftBoundary;
}
