namespace Client.Models;

public sealed class BirdState
{
    public const double UninitializedTimestamp = double.NegativeInfinity;

    public float PositionY { get; set; }

    public float VelocityY { get; set; }

    public bool IsAlive { get; set; } = true;

    public double LastInputTimestamp { get; set; } = UninitializedTimestamp;

    public bool PendingImpulse { get; set; }

    public void Reset(float spawnY)
    {
        PositionY = spawnY;
        VelocityY = 0f;
        IsAlive = true;
        PendingImpulse = false;
        LastInputTimestamp = UninitializedTimestamp;
    }

    public void QueueImpulse(double timestamp)
    {
        PendingImpulse = true;
        LastInputTimestamp = timestamp;
    }

    public void ApplyImpulse(float impulse)
    {
        VelocityY = -impulse;
        PendingImpulse = false;
    }

    public void Kill()
    {
        IsAlive = false;
        VelocityY = 0f;
        PendingImpulse = false;
    }
}
