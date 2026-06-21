namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public interface ILockOnWallTimerService
    {
        void StepTimers(float deltaTime);
        bool IsWallShootableByPlayer(ushort playerId);
        void ResetPlayerTimer(ushort playerId);
    }
}
