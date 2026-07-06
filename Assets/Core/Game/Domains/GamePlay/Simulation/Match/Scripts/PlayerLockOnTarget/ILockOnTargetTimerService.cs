using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public interface ILockOnTargetTimerService
    {
        void AddPlayer(ushort casterId);
        void StepTimers(float deltaTime);
        bool IsTargetShootable(ushort casterId, ushort targetId, LockOnTargetType targetType);
        void ResetTimer(ushort casterId, ushort targetId, LockOnTargetType targetType);
        void ResetAllTimers();
    }
}
