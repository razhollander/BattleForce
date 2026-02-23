using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentLavaWallModel
    {
        public ushort Id;
        public Vector2[] Points;
        public Vector2 LocalPosition;
        public Vector2 WorldPosition;
        public float WorldRotationAngle;
        
        public MatchEnvironmentLavaWallModel(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle)
        {
            Id = id;
            Points = points;
            LocalPosition = localPosition;
            WorldPosition = worldPosition;
            WorldRotationAngle = worldRotationAngle;
        }
    }
}