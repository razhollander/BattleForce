using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public interface IPlayersPowerUpsManager
    {
        void AddPlayer(ushort playerId);
        void RemovePlayer(ushort playerId);
        bool TryGrantPowerUp(ushort playerId, PowerUpType grantedPowerUpType, int tick);
        void ProcessPowerUpInput(ushort playerId, int tick, bool wasPowerUpInputDownThisTick);
        void OnTick(int tick);
        void RemoveAllPowerUps();
        bool IsPowerUpActiveForPlayer(ushort playerId);
    }
}
