using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentSpringModel
    {
        public ushort Id;
        public Vector2 Position;
        public float DirectionAngle;

        public MatchEnvironmentSpringModel(ushort id, Vector2 position, float directionAngle)
        {
            Id = id;
            Position = position;
            DirectionAngle = directionAngle;
        }
    }
}
