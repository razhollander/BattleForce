namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public interface ILockOnWallTimerService
    {
        void StepTimers(float deltaTime);
        bool IsShootable(ushort playerId);
        void ResetTimer(ushort playerId);
    }
}
