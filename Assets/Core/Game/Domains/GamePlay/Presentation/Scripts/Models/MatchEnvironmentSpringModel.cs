using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentSpringModel
    {
        public ushort Id;
        public Vector2 LocalPosition;
        public Vector2 WorldPosition;
        public float LocalDirectionAngle;
        public float WorldDirectionAngle;
        
        public MatchEnvironmentSpringModel(ushort id, Vector2 localPosition, Vector2 worldPosition, float localDirectionAngle, float worldDirectionAngle)
        {
            Id = id;
            LocalPosition = localPosition;
            WorldPosition = worldPosition;
            LocalDirectionAngle = localDirectionAngle;
            WorldDirectionAngle = worldDirectionAngle;
        }
    }
}
