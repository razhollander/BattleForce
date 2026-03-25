using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchKOProjectileModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public int StartTick;
        public float Size;
        public Vector2 Position;
        public Vector2 Rotation;

        public MatchKOProjectileModel(ushort id, ushort casterPlayerId, int startTick, float size)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            StartTick = startTick;
            Size = size;
        }
    }
}
