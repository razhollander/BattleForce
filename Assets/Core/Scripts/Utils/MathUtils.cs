using Core.Scripts.Extensions;
using UnityEngine;

namespace CoreDomain.Scripts.Utils
{
    public static class MathUtils
    {
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
        
        public static System.Numerics.Vector2 GetTeleportedVelocity(System.Numerics.Vector2 currentVelocity, System.Numerics.Vector2 enterNormal, System.Numerics.Vector2 exitNormal)
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
    }
}
