using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentFieldBarrierModel
    {
        public Vector2 Position;
        public Vector2 Size;
        public FieldBarrierShape Shape;
        public ushort TeamId;
        public ushort Id;

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
                    var radius = Size.X;
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
    }
}
