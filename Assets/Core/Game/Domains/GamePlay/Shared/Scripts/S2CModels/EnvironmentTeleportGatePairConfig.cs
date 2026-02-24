namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentTeleportGatePairConfig
    {
        private const ushort GateCount = 2;
        
        public ushort Id;
        public EnvironmentTeleportGateS2C GateA;
        public EnvironmentTeleportGateS2C GateB;
        public ushort GateAId => (ushort) (Id * GateCount);
        public ushort GateBId => (ushort) (Id * GateCount + 1);
    }
}