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
            float t = 1f - Mathf.Exp(-decay * deltaTime);

            return Quaternion.Slerp(a, b, t);
        }
    }
}
