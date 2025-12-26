using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class UnityVectorExtensions
    {
        public static Vector2 ToUnityVector2(this System.Numerics.Vector2 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }
        
        public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 vec)
        {
            return new System.Numerics.Vector2(vec.x, vec.y);
        }
        
        public static Vector2 ToVector2XY(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static Vector2 ToVector2XZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        
        public static Vector2 Rotate(this Vector2 direction, float degrees)
        {
            return Quaternion.Euler(0, 0, degrees) * direction;
        }
        
        public static System.Numerics.Vector2 Rotate(this System.Numerics.Vector2 direction, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);

            return new System.Numerics.Vector2(
                direction.X * cos - direction.Y * sin,
                direction.X * sin + direction.Y * cos
            );
        }

        
        public static Quaternion ToQuaternion(this Vector2 direction)
        {
            // note: in math the angle of (1,0) is 0, then going anti-clockwise
            return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
    }
}