using Client.Models;
using Client.Services.Physics;

namespace Client.Services;

public sealed class ObstacleSpawner
{
    public const float MinimumGap = 110f;

    public const float MaximumGap = 160f;

    public const float PipeSpeed = -120f;

    public const float SpawnX = 800f;

    public const float PlayfieldHeight = PhysicsEngine.GroundLevel + 12f;

    public const float MinimumPipeLength = 32f;

    private readonly IRngProvider _rngProvider;

    public ObstacleSpawner(IRngProvider rngProvider)
    {
        ArgumentNullException.ThrowIfNull(rngProvider);
        _rngProvider = rngProvider;
    }

    public PipePair SpawnNext()
    {
        float gapRatio = _rngProvider.NextFloat();
        float gapSize = MinimumGap + ((MaximumGap - MinimumGap) * gapRatio);
        float halfGap = gapSize * 0.5f;

        float minCenter = halfGap + MinimumPipeLength;
        float maxCenter = Math.Max(minCenter, PlayfieldHeight - (halfGap + MinimumPipeLength));
        float gapCenter;

        if (maxCenter <= minCenter)
        {
            gapCenter = PlayfieldHeight * 0.5f;
        }
        else
        {
            float span = maxCenter - minCenter;
            float centerRatio = _rngProvider.NextFloat();
            gapCenter = minCenter + (span * centerRatio);
        }

        return new PipePair
        {
            PositionX = SpawnX,
            VelocityX = PipeSpeed,
            GapSize = gapSize,
            GapCenterY = gapCenter,
            Passed = false,
        };
    }
}
