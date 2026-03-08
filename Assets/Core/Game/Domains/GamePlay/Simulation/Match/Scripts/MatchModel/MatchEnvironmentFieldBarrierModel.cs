using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentFieldBarrierModel
    {
        public Vector2 Position;
        public Vector2 Size;
        public FieldBarrierShape Shape;
        public ushort TeamId;
        public ushort Id;
        public float CircleRadius => Size.X;

        public MatchEnvironmentFieldBarrierModel()
        {
        }
        
        public bool IsPointInsideBarrier(Vector2 point)
        {
            switch (Shape)
            {
                case FieldBarrierShape.Circle:
                {
                    var center = Position;
                    var radius = CircleRadius;
                    return Vector2.DistanceSquared(point, center) <= radius * radius;
                }
                case FieldBarrierShape.Rectangle:
                {
                    var center = Position;
                    var halfSize = Size * 0.5f;
                    var min = center - halfSize;
                    var max = center + halfSize;
                    return point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y;
                }
                default:
                    return false;
            }
        }
        
        public bool IsCircleInsideBarrier(Vector2 circleCenter, float circleRadius)
        {
            switch (Shape)
            {
                case FieldBarrierShape.Circle:
                {
                    var center = Position;
                    var barrierRadius = CircleRadius;
                    if (barrierRadius < circleRadius)
                    {
                        LogService.LogError($"Barrier radius {barrierRadius} is smaller than circle radius {circleRadius}. This should never happen.");
                        return false;
                    }

                    var maxAllowedDistance = barrierRadius - circleRadius;
                    return Vector2.DistanceSquared(circleCenter, center) <= maxAllowedDistance * maxAllowedDistance;
                }
                case FieldBarrierShape.Rectangle:
                {
                    var center = Position;
                    var halfSize = Size * 0.5f;
            
                    // Shrink the safe area by the point's radius
                    var safeHalfWidth = Math.Max(0, halfSize.X - circleRadius);
                    var safeHalfHeight = Math.Max(0, halfSize.Y - circleRadius);

                    var minX = center.X - safeHalfWidth;
                    var maxX = center.X + safeHalfWidth;
                    var minY = center.Y - safeHalfHeight;
                    var maxY = center.Y + safeHalfHeight;

                    return circleCenter.X >= minX && circleCenter.X <= maxX && circleCenter.Y >= minY && circleCenter.Y <= maxY;
                }
                default:
                    return false;
            }
        }
    }
}
