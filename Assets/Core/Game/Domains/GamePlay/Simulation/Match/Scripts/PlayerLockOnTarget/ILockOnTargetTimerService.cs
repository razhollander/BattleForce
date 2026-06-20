namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public interface ILockOnTargetTimerService
    {
        void AddPlayer(ushort casterId);
        void StepTimers(float deltaTime);
        bool IsTargetShootable(ushort casterId, ushort targetId);
        void ResetTimer(ushort casterId, ushort targetId);
        void ResetAllTimers();
    }
}
