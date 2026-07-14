using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchFishingRodTipModel
    {
        public readonly ushort Id;
        public readonly ushort CasterPlayerId;
        public Vector2 Position;
        public FishingRodTipPhase Phase;
        public Vector2 EnemyCaughtArrowDirection;

        public MatchFishingRodTipModel(ushort id, ushort casterPlayerId, Vector2 position, FishingRodTipPhase phase)
        {
            Id = id;
            CasterPlayerId = casterPlayerId;
            Position = position;
            Phase = phase;
        }
    }
}
