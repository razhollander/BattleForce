using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public struct PowerUpInGrantingPhase
    {
        public readonly int GrantingPhaseEndTick;
        public readonly PowerUpType PowerUpType;

        public PowerUpInGrantingPhase(int grantingPhaseEndTick, PowerUpType powerUpType)
        {
            GrantingPhaseEndTick = grantingPhaseEndTick;
            PowerUpType = powerUpType;
        }
    }
}
