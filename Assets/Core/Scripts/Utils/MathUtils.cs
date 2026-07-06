using System;
using Core.Scripts.Extensions;
using UnityEngine;

namespace CoreDomain.Scripts.Utils
{
    public static class MathUtils
    {
        // Threshold in radians (e.g., 45 degrees is ~0.785 radians)
        private const float TurnThresholdRad = 0.1f;
        private const float TwoPI = MathF.PI * 2f;

        public static Vector3 GetPerpendicularDirection(Vector2 startPoint, Vector2 endPoint)
        {
            var direction = (endPoint - startPoint);

            return GetPerpendicularDirection(direction);
        }

        public static Vector2 GetPerpendicularDirection(Vector2 direction)
        {
            var normalizedDir = direction.normalized;

            return new Vector2(-normalizedDir.y, normalizedDir.x);
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

        /// <summary>
        /// Rotates a Vector2 towards a target Vector2 by a maximum degree delta.
        /// </summary>
        public static System.Numerics.Vector2 RotateTowards(System.Numerics.Vector2 current, System.Numerics.Vector2 target, float maxDegreesDelta)
        {
            float currentRad = GetAngle(current);
            float targetRad = GetAngle(target);

            // Calculate the shortest signed difference between angles
            float deltaRad = DeltaSignedAngleRadians(currentRad, targetRad);

            // Convert max delta to radians and clamp the rotation
            float maxRadDelta = maxDegreesDelta * (MathF.PI / 180f);
            float actualRotation = Math.Clamp(deltaRad, -maxRadDelta, maxRadDelta);

            return Rotate(current, actualRotation);
        }

        /// <summary>
        /// Returns the angle of the vector in radians using Atan2.
        /// </summary>
        public static float GetAngle(System.Numerics.Vector2 vector)
        {
            return MathF.Atan2(vector.Y, vector.X);
        }

        /// <summary>
        /// Calculates the shortest difference between two radian angles, 
        /// wrapping correctly around the 2PI boundary.
        /// </summary>
        public static float DeltaSignedAngleRadians(float current, float target)
        {
            float diff = target - current;

            // Wrap the angle to -PI to PI range
            while (diff > MathF.PI) diff -= MathF.PI * 2;
            while (diff < -MathF.PI) diff += MathF.PI * 2;

            return diff;
        }
        
        public static float DeltaAbsoluteAngleRadians(float current, float target)
        {
            var diff = MathF.Abs(target - current) % TwoPI;
            return diff > MathF.PI ? TwoPI - diff : diff;
        }
        
        /// <summary>
        /// Rotates a vector by a specific radian amount.
        /// </summary>
        public static System.Numerics.Vector2 Rotate(System.Numerics.Vector2 v, float radians)
        {
            float ca = MathF.Cos(radians);
            float sa = MathF.Sin(radians);

            return new System.Numerics.Vector2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
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
        
        public static System.Numerics.Vector2 GetClosestPointOnSegment(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, System.Numerics.Vector2 point)
            {
                var l2 = System.Numerics.Vector2.DistanceSquared(p1, p2);

                if (l2 == 0) return p1;

                var t = Math.Max(0, Math.Min(1, System.Numerics.Vector2.Dot(point - p1, p2 - p1) / l2));

                return p1 + t * (p2 - p1);
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
                var v1 = currentMovementDirection.NormalizeSafe();
                var v2 = desiredMovementDiretion.NormalizeSafe();

                // 3. The Math:
                // Dot product = cos(theta)
                // Determinant (Perp-Dot) = sin(theta)
                float dot = System.Numerics.Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                // Atan2 returns the signed angle in radians (-PI to PI).
                // This is inherently the shortest arc between the two vectors.
                float shortestAngleInRadians = (float) Math.Atan2(det, dot);

                // 4. Threshold Comparison
                // Positive result means the target is to the "Left" (Counter-Clockwise)
                // Negative result means the target is to the "Right" (Clockwise)
                bool shouldChangeLeft = shortestAngleInRadians > TurnThresholdRad;
                bool shouldChangeRight = shortestAngleInRadians < -TurnThresholdRad;

                return (shouldChangeRight, shouldChangeLeft);
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
                float angleInto = (float) (Mathf.Atan2(enterNormal.Y, enterNormal.X) * 180.0 / Mathf.PI);
                float angleOut = (float) (Mathf.Atan2(exitNormal.Y, exitNormal.X) * 180.0 / Mathf.PI);

                // This is the degree difference in CCW
                float deltaDegrees = angleOut - angleInto;

                // 3. Rotate the local offset so it is oriented correctly relative to the exit's facing
                var rotatedOffset = localOffset.Rotate(deltaDegrees);

                // 4. Position the entity relative to the exit portal's center
                return exitCenter + rotatedOffset;
            }

            public static System.Numerics.Vector2 ConvertVectorTelativeToExitTeleport(System.Numerics.Vector2 currentVelocity, System.Numerics.Vector2 enterNormal,
                System.Numerics.Vector2 exitNormal)
            {
                // 1. The direction 'into' the entrance is the opposite of its normal
                var entranceForward = -enterNormal;

                // 2. Calculate the angle of both directions in degrees
                // Using Atan2 to get the full 360-degree range
                float angleEntrance = (float) (Mathf.Atan2(entranceForward.Y, entranceForward.X) * (180 / Mathf.PI));
                float angleExit = (float) (Mathf.Atan2(exitNormal.Y, exitNormal.X) * (180 / Mathf.PI));

                // 3. Find the difference (The rotation required to get from Entrance to Exit)
                float deltaDegrees = angleExit - angleEntrance;

                // 4. Use your Rotate method to transform the velocity by that difference
                return currentVelocity.Rotate(deltaDegrees);
            }
        }
    }
}

