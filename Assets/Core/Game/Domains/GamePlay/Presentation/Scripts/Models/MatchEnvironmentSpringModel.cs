using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentSpringModel
    {
        public ushort Id;
        public Vector2 LocalPosition;
        public Vector2 WorldPosition;
        public float LocalRotationAngle;
        public float WorldRotationAngle;

        public float WorldDirectionAngle
        {
            get { return WorldRotationAngle-90; }
        }
        
        public MatchEnvironmentSpringModel(ushort id, Vector2 localPosition, Vector2 worldPosition, float localRotationAngle, float worldRotationAngle)
        {
            Id = id;
            LocalPosition = localPosition;
            WorldPosition = worldPosition;
            LocalRotationAngle = localRotationAngle;
            WorldRotationAngle = worldRotationAngle;
        }
    }
}
