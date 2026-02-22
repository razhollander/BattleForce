using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class EnvironmentRotatingWheelUtils
    {
        public static float CalculateRotation(int tick, float rotationSpeed, float deltaTime)
        {
            return rotationSpeed * tick * deltaTime;
        }

        public static Vector2 Rotate(Vector2 point, float degrees)
        {
            float radians = degrees * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(
                point.X * cos - point.Y * sin,
                point.X * sin + point.Y * cos
            );
        }

        public static void CalculateChildTransform(
            int tick,
            float rotationSpeed,
            float deltaTime,
            Vector2 wheelCenter,
            Vector2 initialChildLocalPos,
            float initialChildLocalRot, // degrees
            out Vector2 newPos,
            out float newRot)
        {
            float angle = CalculateRotation(tick, rotationSpeed, deltaTime);
            newPos = Rotate(initialChildLocalPos, angle) + wheelCenter;
            newRot = initialChildLocalRot + angle;
        }
    }
}
