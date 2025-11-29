using System;
using System.Numerics;

namespace Core.Scripts.Extensions
{
    public class QuaternionExtensions
    {
        private static readonly float ToAngleFactor = 180f / MathF.PI;
        
        public static float ToAngle(Quaternion q)
        {
            var siny = 2f * (q.W * q.Z + q.X * q.Y);
            var cosy = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
            var angleRad = MathF.Atan2(siny, cosy);
            return angleRad * ToAngleFactor;
        }
    }
}