using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchSwapFieldModel
    {
        public int OccuredOnTick { get; private set; }
        public ushort Id { get; private set; }
        public ushort PlayerCasterId { get; private set; }
        public int EndTick { get; private set; }
        public float MaxRadius { get; private set; }

        public MatchSwapFieldModel(ushort id, ushort playerCasterId, int occuredOnTick, int endTick, float maxRadius)
        {
            Id = id;
            PlayerCasterId = playerCasterId;
            OccuredOnTick = occuredOnTick;
            EndTick = endTick;
            MaxRadius = maxRadius;
        }

        public float CalculateCurrentRadiusForTick(int tick)
        {
            return MathUtils.RemapClamped(OccuredOnTick, EndTick, 0, MaxRadius, tick);
        }
    }
}