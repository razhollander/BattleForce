using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchSoulGhostModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;
        public Vector2 Direction;

        public MatchSoulGhostModel(ushort id, ushort casterPlayerId, Vector2 position, Vector2 direction)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
            Direction = direction;
        }
    }
}
