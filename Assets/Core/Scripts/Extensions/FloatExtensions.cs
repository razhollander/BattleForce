using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class FloatExtensions
    {
        public static Quaternion AngleToQuaternion(this float angle)
        {
            return Quaternion.Euler(0f, 0f, angle);    
        }
        
        public static System.Numerics.Vector2 AngleToRadians(this float angleRadians)
        {
            return new System.Numerics.Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));    
        }
    }
}