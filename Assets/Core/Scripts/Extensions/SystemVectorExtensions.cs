using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Scripts.Extensions
{
    public static class SystemVectorExtensions
    {
        public static float ToAngle(this Vector2 direction)
        {
            return Mathf.Atan2(direction.Y, direction.X) * Mathf.Rad2Deg;
        }
        
        public static Vector2 ReflectFromWall(this Vector2 direction, Vector2 wallNormal)
        {
            return direction - 2 * Vector2.Dot(direction, wallNormal) * wallNormal;
        }
    }
}