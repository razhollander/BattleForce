using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
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