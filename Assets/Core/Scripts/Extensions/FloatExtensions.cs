using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class FloatExtensions
    {
        public static Quaternion AngleToQuaternion(this float angle)
        {
            return Quaternion.Euler(0f, 0f, angle);    
        }
    }
}