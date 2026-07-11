using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchFishingRodTipModel
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;
        public FishingRodTipPhase Phase;
        // World-space direction of the throw-aim arrow shown on the caught enemy while the tip is in the CaughtEnemy phase.
        public Vector2 EnemyCaughtArrowDirection;

        public MatchFishingRodTipModel(ushort id, ushort casterPlayerId, Vector2 position)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
        }
    }
}
