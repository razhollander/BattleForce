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
    }
}