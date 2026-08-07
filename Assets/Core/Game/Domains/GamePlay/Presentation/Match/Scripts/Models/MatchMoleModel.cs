using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchMoleModel
    {
        public ushort Id;
        public Vector2 Position;
        public bool IsGolden;
        public byte RemainingLives;
        public byte MaxLives;

        public MatchMoleModel(ushort id, Vector2 position, bool isGolden, byte remainingLives, byte maxLives)
        {
            Id = id;
            Position = position;
            IsGolden = isGolden;
            RemainingLives = remainingLives;
            MaxLives = maxLives;
        }
    }
}
