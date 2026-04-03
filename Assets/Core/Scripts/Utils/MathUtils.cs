using System;
using Core.Scripts.Extensions;
using UnityEngine;

namespace CoreDomain.Scripts.Utils
{
    public static class MathUtils
    {
        // Threshold in radians (e.g., 45 degrees is ~0.785 radians)
        private const float TurnThresholdRad = 0.1f;
        
        public static Vector3 GetPerpendicularDirection(Vector2 startPoint, Vector2 endPoint)
        {
            var direction = (endPoint - startPoint);
            return GetPerpendicularDirection(direction);
        }
            
        public static Vector3 GetPerpendicularDirection(Vector2 direction)
        {
            var normalizedDir = direction.normalized;
            return new Vector3(-normalizedDir.y, normalizedDir.x, 0);
        }
        
        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        public static float Remap(float inMin, float inMax, float outMin, float outMax, float value)
        {
            if (Mathf.Approximately(inMin, inMax)) return outMin;

            return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
        }

        /// <summary>
        /// Remaps a value from one range to another, ensuring the result stays within the output range.
        /// </summary>
        public static float RemapClamped(float inMin, float inMax, float outMin, float outMax, float value)
        {
            if (Mathf.Approximately(inMin, inMax)) return outMin;
            var t = (value - inMin) / (inMax - inMin);
            t = Math.Max(0f, Math.Min(1f, t)); 
            return outMin + t * (outMax - outMin);
        }
        
        public static bool DidCrossTargetAngle(float previousAngle, float currentAngle, float targetAngle, float tolerance)
        {
            bool wasPreviousAbove = previousAngle < targetAngle + tolerance && previousAngle > targetAngle;
            bool isCurrentBelow = currentAngle > targetAngle - tolerance && currentAngle <= targetAngle;
            return wasPreviousAbove && isCurrentBelow;
        }
        
        public static Vector3 RotateVectorRelativeToSurface(Vector3 vector, Vector3 surfaceNormal)
        {
            var rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
            return rotation * vector;
        }

        public static Vector2 ExpDecay(Vector2 a, Vector2 b, float decay, float deltaTime)
        {
            return b + (a - b) * Mathf.Exp(-decay * deltaTime);
        }
        
        public static float ExpDecay(float a, float b, float decay, float deltaTime)
        {
            return b + (a - b) * Mathf.Exp(-decay * deltaTime);
        }
        
        public static Quaternion ExpDecay(Quaternion a, Quaternion b, float decay, float deltaTime)
        {
            // Mathf.Exp(-decay * deltaTime) calculates how much of the "residual" 
            // rotation 'a' should remain. 
            // 1 - that value is the interpolation factor (t) for the Slerp.
            // https://www.youtube.com/watch?v=LSNQuFEDOyQ
            float t = 1f - Mathf.Exp(-decay * deltaTime);

            return Quaternion.Slerp(a, b, t);
        }

        public static class TeleportsLogic
        {
            /// <summary>
            /// Calculates the world-space exit point based on where the entity hit the entrance.
            /// </summary>
            /// <param name="collisionPoint">The exact world position of the collision.</param>
            /// <param name="enterCenter">The center point of the entrance portal.</param>
            /// <param name="enterNormal">The surface normal of the entrance portal (facing out).</param>
            /// <param name="exitCenter">The center point of the target portal.</param>
            /// <param name="exitNormal">The surface normal of the target portal (facing out).</param>
            /// <returns>The translated world-space position at the exit portal.</returns>
            public static System.Numerics.Vector2 GetRelativeExitPoint(
                System.Numerics.Vector2 collisionPoint, 
                System.Numerics.Vector2 enterCenter, 
                System.Numerics.Vector2 enterNormal, 
                System.Numerics.Vector2 exitCenter, 
                System.Numerics.Vector2 exitNormal)
            {
                // 1. Get the offset from the center of the entrance portal
                var localOffset = collisionPoint - enterCenter;

                // 2. Calculate the rotation delta.
                // We want to map the direction "into" the entrance (-enterNormal)
                // to the direction "out of" the exit (exitNormal).
                float angleInto = (float)(Mathf.Atan2(enterNormal.Y, enterNormal.X) * 180.0 / Mathf.PI);
                float angleOut = (float)(Mathf.Atan2(exitNormal.Y, exitNormal.X) * 180.0 / Mathf.PI);

                // This is the degree difference in CCW
                float deltaDegrees = angleOut - angleInto;

                // 3. Rotate the local offset so it is oriented correctly relative to the exit's facing
                var rotatedOffset = localOffset.Rotate(deltaDegrees);

                // 4. Position the entity relative to the exit portal's center
                return exitCenter + rotatedOffset;
            }
            
            public static System.Numerics.Vector2 ConvertVectorTelativeToExitTeleport(System.Numerics.Vector2 currentVelocity, System.Numerics.Vector2 enterNormal, System.Numerics.Vector2 exitNormal)
            {
                // 1. The direction 'into' the entrance is the opposite of its normal
                var entranceForward = -enterNormal;

                // 2. Calculate the angle of both directions in degrees
                // Using Atan2 to get the full 360-degree range
                float angleEntrance = (float)(Mathf.Atan2(entranceForward.Y, entranceForward.X) * (180 / Mathf.PI));
                float angleExit = (float)(Mathf.Atan2(exitNormal.Y, exitNormal.X) * (180 / Mathf.PI));

                // 3. Find the difference (The rotation required to get from Entrance to Exit)
                float deltaDegrees = angleExit - angleEntrance;

                // 4. Use your Rotate method to transform the velocity by that difference
                return currentVelocity.Rotate(deltaDegrees);
            }

            public static (bool shouldChangeDirectionLocalRight, bool shouldChangeDirectionLocalLeft) GetDirectionChangeInputs(
                System.Numerics.Vector2 currentMovementDirection, 
                System.Numerics.Vector2 desiredMovementDiretion)
            {
                // 1. Deadzone check: avoid jitter or NaN errors when stick is barely touched
                if (currentMovementDirection.LengthSquared() < 0.01f || desiredMovementDiretion.LengthSquared() < 0.01f)
                {
                    return (false, false);
                }

                // 2. Normalize to treat these as pure directions
                var v1 = System.Numerics.Vector2.Normalize(currentMovementDirection);
                var v2 = System.Numerics.Vector2.Normalize(desiredMovementDiretion);

                // 3. The Math:
                // Dot product = cos(theta)
                // Determinant (Perp-Dot) = sin(theta)
                float dot = System.Numerics.Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                // Atan2 returns the signed angle in radians (-PI to PI).
                // This is inherently the shortest arc between the two vectors.
                float shortestAngle = (float)Math.Atan2(det, dot);

                // 4. Threshold Comparison
                // Positive result means the target is to the "Left" (Counter-Clockwise)
                // Negative result means the target is to the "Right" (Clockwise)
                bool shouldChangeLeft = shortestAngle > TurnThresholdRad;
                bool shouldChangeRight = shortestAngle < -TurnThresholdRad;

                return (shouldChangeRight, shouldChangeLeft);
            }
        }
    }
}
