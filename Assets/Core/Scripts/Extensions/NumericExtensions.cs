using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class NumericExtensions
    {
        public static Vector2 ToUnity(this System.Numerics.Vector2 vector2)
        {
            return new Vector2(vector2.X, vector2.Y);
        }
    }
}