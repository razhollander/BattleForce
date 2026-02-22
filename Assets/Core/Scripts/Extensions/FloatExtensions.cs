using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class FloatExtensions
    {
        public static Quaternion AngleToQuaternion(this float angleDegrees)
        {
            return Quaternion.Euler(0f, 0f, angleDegrees);    
        }
        
        public static System.Numerics.Vector2 AngleToVector(this float angleRadians)
        {
            return new System.Numerics.Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));    
        }
        
        public static float ToRadians(this float angleDegrees)
        {
            return angleDegrees * Mathf.Deg2Rad;    
        }
    }
}