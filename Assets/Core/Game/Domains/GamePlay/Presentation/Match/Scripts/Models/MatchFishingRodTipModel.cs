using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchFishingRodTipModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;

        public MatchFishingRodTipModel(ushort id, ushort casterPlayerId, Vector2 position)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
        }
    }
}
