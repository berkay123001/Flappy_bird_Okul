using Client.Models;

namespace Client.Services.Physics;

public sealed class PhysicsEngine
{
    public const float Gravity = 2000f;

    public const float JumpImpulse = 450f;

    public const float GroundLevel = 488f;

    public BirdState AdvanceBird(BirdState bird, float deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(bird);

        if (deltaSeconds <= 0f || !bird.IsAlive)
        {
            return bird;
        }

        float velocity = bird.VelocityY;
        bool pendingImpulse = bird.PendingImpulse;
        bool isAlive = bird.IsAlive;
        float position = bird.PositionY;

        if (pendingImpulse)
        {
            velocity = -JumpImpulse;
            pendingImpulse = false;
        }

        velocity += Gravity * deltaSeconds;
        position += velocity * deltaSeconds;

        if (position >= GroundLevel)
        {
            position = GroundLevel;
            isAlive = false;
            velocity = 0f;
            pendingImpulse = false;
        }

        return new BirdState
        {
            PositionY = position,
            VelocityY = velocity,
            IsAlive = isAlive,
            PendingImpulse = pendingImpulse,
            LastInputTimestamp = bird.LastInputTimestamp,
        };
    }
}
