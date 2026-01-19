using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchData.Models
{
    public class MatchPowerUpBallModel
    {
        public ushort Id;
        public Vector2 Position;

        public MatchPowerUpBallModel(ushort id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }
}