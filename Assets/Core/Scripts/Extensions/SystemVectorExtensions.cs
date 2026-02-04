using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Scripts.Extensions
{
    public static class SystemVectorExtensions
    {
        public static float ToAngleRadians(this Vector2 direction)
        {
            return Mathf.Atan2(direction.Y, direction.X);
        }
        
        public static Vector2 FromAngleRadians(this float angle)
        {
            return new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            );
        }
        
        public static Vector2 ReflectFromWall(this Vector2 direction, Vector2 wallNormal)
        {
            return direction - 2 * Vector2.Dot(direction, wallNormal) * wallNormal;
        }
        
        public static bool IsFacingWall(this Vector2 direction, Vector2 wallNormal)
        {
            return Vector2.Dot(direction, wallNormal) < 0;
        }

        public static Vector2 Normalize(this Vector2 v)
        {
            return Vector2.Normalize(v);
        }
    }
}