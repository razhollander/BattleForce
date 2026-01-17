using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchData.Models
{
    public class MatchTalentCardModel
    {
        public ushort Id;
        public Vector2 Position;
        public TalentType TalentType;
        public ushort Health;

        public MatchTalentCardModel(ushort id, Vector2 position, TalentType talentType, ushort health)
        {
            Id = id;
            Position = position;
            TalentType = talentType;
            Health = health;
        }
    }
}