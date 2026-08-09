using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchScoreGateModel
    {
        public ushort Id;
        public Vector2 Position;
        public Vector2 Rotation;
        public ushort LastScoredTeamId; // 0 = never scored; drives the gate tint

        public MatchScoreGateModel(ushort id, Vector2 position, Vector2 rotation, ushort lastScoredTeamId)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            LastScoredTeamId = lastScoredTeamId;
        }
    }
}
