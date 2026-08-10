using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchScoreGateModel
    {
        public ushort Id;
        public Vector2 Position;
        public Vector2 Rotation;
        public ushort LastScoredTeamId; // 0 = never scored; drives the gate tint
        public byte ScoreMultiplier; // multiplier the next pass will award; drives the x2/x3/x4 indicator

        public MatchScoreGateModel(ushort id, Vector2 position, Vector2 rotation, ushort lastScoredTeamId, byte scoreMultiplier)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            LastScoredTeamId = lastScoredTeamId;
            ScoreMultiplier = scoreMultiplier;
        }
    }
}
