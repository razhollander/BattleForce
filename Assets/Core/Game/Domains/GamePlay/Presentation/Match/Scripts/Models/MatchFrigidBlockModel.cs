using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchFrigidBlockModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;
        public Vector2 Rotation;

        public MatchFrigidBlockModel(ushort id, ushort casterPlayerId, Vector2 position, Vector2 rotation)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
            Rotation = rotation;
        }
    }
}
