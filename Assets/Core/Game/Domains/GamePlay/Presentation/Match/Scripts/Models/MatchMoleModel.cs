namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchMoleModel
    {
        public readonly ushort Id;
        public readonly ushort MoleHoleId;
        public readonly bool IsGolden;
        public readonly byte MaxLives;
        public byte RemainingLives;

        public MatchMoleModel(ushort id, ushort moleHoleId, bool isGolden, byte remainingLives, byte maxLives)
        {
            Id = id;
            MoleHoleId = moleHoleId;
            IsGolden = isGolden;
            RemainingLives = remainingLives;
            MaxLives = maxLives;
        }
    }
}
