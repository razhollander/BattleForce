using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Models
{
    public class MatchEnvironmentSpringModel
    {
        public ushort Id;
        public Vector2 Position;
        public float Rotation;
        public Vector2 Size;

        public MatchEnvironmentSpringModel(ushort id, Vector2 position, float rotation, Vector2 size)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            Size = size;
        }
    }
}
