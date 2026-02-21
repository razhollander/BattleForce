using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentSpringModel
    {
        public ushort Id;
        public Vector2 Position;
        public float Rotation;

        public MatchEnvironmentSpringModel(ushort id, Vector2 position, float rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }
    }
}
