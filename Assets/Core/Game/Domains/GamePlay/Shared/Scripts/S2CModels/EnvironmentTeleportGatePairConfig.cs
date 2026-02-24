using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentTeleportGatePairConfig
    {
        private const ushort GateCount = 2;
        
        public ushort Id;
        public EnvironmentTeleportGateConfig GateA;
        public EnvironmentTeleportGateConfig GateB;
        public ushort GateAId => (ushort) (Id * GateCount);
        public ushort GateBId => (ushort) (Id * GateCount + 1);
    }

    public struct EnvironmentTeleportGateConfig
    {
        public Vector2 Position;
        public float NormalRotation;
    }
}