using System.Numerics;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class EnvironmentRotatingWheelUtils
    {
        public static float CalculateRotationDuringTick(int tick, float rotationSpeed, float deltaTime)
        {
            // Use double to maintain precision for large tick values (e.g., > 16 million)
            // and normalize the angle to avoid large float values in subsequent calculations.
            double totalRotation = (double)tick * rotationSpeed * deltaTime;
            return (float)(totalRotation % 360.0);
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

            // Normalize the final rotation as well to keep it within bounds
            newRotation = (initialChildLocalRotatingDegrees + angleDegrees) % 360f;
        }
    }
}
