using System.Numerics;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateModel
    {
        public ushort PairId { get; private set; }
        public bool IsGateB { get; private set; }
        public Vector2 Position { get; private set; }
        public float Rotation { get; private set; }

        public EnvironmentTeleportGateModel(ushort pairId, bool isGateB, Vector2 position, float rotation)
        {
            PairId = pairId;
            IsGateB = isGateB;
            Position = position;
            Rotation = rotation;
        }
    }
}
