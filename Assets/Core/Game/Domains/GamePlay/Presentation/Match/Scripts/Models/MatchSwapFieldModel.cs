using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchSwapFieldModel
    {
        public ushort Id { get; private set; }
        public ushort PlayerCasterId { get; private set; }
        public int CreatedOnTick { get; private set; }
        public int EndTick { get; private set; }
        public float MaxRadius { get; private set; }

        public MatchSwapFieldModel(ushort id, ushort playerCasterId, int createdOnTick, int endTick, float maxRadius)
        {
            Id = id;
            PlayerCasterId = playerCasterId;
            CreatedOnTick = createdOnTick;
            EndTick = endTick;
            MaxRadius = maxRadius;
        }

        public float CalculateCurrentRadiusForTick(int tick)
        {
            return MathUtils.Remap(CreatedOnTick, EndTick, 0, MaxRadius, tick);
        }
    }
}