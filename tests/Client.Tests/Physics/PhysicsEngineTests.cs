using Client.Models;
using Client.Services.Physics;

namespace Client.Tests.Physics;

public sealed class PhysicsEngineTests
{
    private const float DeltaSeconds = 0.016f; // ~16 ms

    [Fact]
    public void AdvanceBirdAppliesGravity()
    {
        PhysicsEngine engine = new();
        BirdState bird = new()
        {
            PositionY = 100f,
            VelocityY = 0f,
            IsAlive = true,
            PendingImpulse = false,
        };

        BirdState next = engine.AdvanceBird(bird, DeltaSeconds);

        float expectedVelocity = PhysicsEngine.Gravity * DeltaSeconds;
        float expectedPosition = bird.PositionY + (expectedVelocity * DeltaSeconds);

        Assert.Equal(expectedVelocity, next.VelocityY, 3);
        Assert.Equal(expectedPosition, next.PositionY, 3);
        Assert.False(next.PendingImpulse);
        Assert.True(next.IsAlive);
    }

    [Fact]
    public void AdvanceBirdConsumesPendingImpulse()
    {
        PhysicsEngine engine = new();
        BirdState bird = new()
        {
            PositionY = 200f,
            VelocityY = 50f,
            IsAlive = true,
            PendingImpulse = true,
        };

        BirdState next = engine.AdvanceBird(bird, DeltaSeconds);

        float expectedVelocity = -PhysicsEngine.JumpImpulse + (PhysicsEngine.Gravity * DeltaSeconds);
        float expectedPosition = bird.PositionY + (expectedVelocity * DeltaSeconds);

        Assert.Equal(expectedVelocity, next.VelocityY, 3);
        Assert.Equal(expectedPosition, next.PositionY, 3);
        Assert.False(next.PendingImpulse);
    }

    [Fact]
    public void AdvanceBirdClampsAtGroundAndStops()
    {
        PhysicsEngine engine = new();
        BirdState bird = new()
        {
            PositionY = PhysicsEngine.GroundLevel - 5f,
            VelocityY = 400f,
            IsAlive = true,
            PendingImpulse = false,
        };

        BirdState next = engine.AdvanceBird(bird, 0.5f);

        Assert.Equal(PhysicsEngine.GroundLevel, next.PositionY, 3);
        Assert.False(next.IsAlive);
        Assert.Equal(0f, next.VelocityY, 3);
    }

    [Fact]
    public void AdvanceBirdKeepsDeadBirdUnchanged()
    {
        PhysicsEngine engine = new();
        BirdState bird = new()
        {
            PositionY = 150f,
            VelocityY = 0f,
            IsAlive = false,
            PendingImpulse = false,
        };

        BirdState next = engine.AdvanceBird(bird, DeltaSeconds);

        Assert.Same(bird, next);
    }
}
