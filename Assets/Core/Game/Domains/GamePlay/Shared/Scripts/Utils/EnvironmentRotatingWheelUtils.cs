using System.Numerics;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class EnvironmentRotatingWheelUtils
    {
        public static float CalculateRotationDuringTick(int tick, float rotationSpeed, float deltaTime)
        {
            return rotationSpeed * tick * deltaTime;
        }

        public static void CalculateChildTransform(
            int tick,
            float rotationSpeed,
            float deltaTime,
            Vector2 wheelCenter,
            Vector2 initialChildLocalPosition,
            float initialChildLocalRotatingDegrees,
            out Vector2 newPosition,
            out float newRotation)
        {
            var angleDegrees = CalculateRotationDuringTick(tick, rotationSpeed, deltaTime);
            newPosition = initialChildLocalPosition.Rotate(angleDegrees) + wheelCenter;
            newRotation = initialChildLocalRotatingDegrees + angleDegrees;
        }
    }
}
