namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public interface IPlayersPowerUpsManager
    {
        void AddPlayer(ushort playerId);
        void RemovePlayer(ushort playerId);
        bool TryGrantRandomPowerUp(ushort playerId, int tick);
        void ProcessPowerUpInput(ushort playerId, int tick, bool wasPowerUpInputDownThisTick);
        void OnTick(int tick);
        void RemoveAllPowerUps();
        bool IsPlayerAimingPowerUp(ushort playerId);
        bool IsPerformInProgressForPlayer(ushort playerId);
    }
}
