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
    }
}
