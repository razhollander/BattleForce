using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.LockOnHeartTargetService
{
    public interface ILockOnHeartTargetService
    {
        void Process(int processedTick, PlayerStateS2C casterPlayerState);
    }
}
