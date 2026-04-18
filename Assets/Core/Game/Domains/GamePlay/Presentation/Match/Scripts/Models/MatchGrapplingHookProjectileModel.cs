using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchGrapplingHookProjectileModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;

        public MatchGrapplingHookProjectileModel(ushort id, ushort casterPlayerId, Vector2 position)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
        }
    }
}
