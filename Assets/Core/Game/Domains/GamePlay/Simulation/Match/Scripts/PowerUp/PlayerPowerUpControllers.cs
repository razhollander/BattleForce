using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public class PlayerPowerUpControllers
    {
        private readonly SonicSlapPowerUpController _sonicSlapPowerUpController;

        public PlayerPowerUpControllers(IMatchDataService matchDataService)
        {
            _sonicSlapPowerUpController = new SonicSlapPowerUpController(matchDataService);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _sonicSlapPowerUpController.SetCasterId(casterPlayerId);
        }

        public void Perform(PowerUpType powerUpType, int tick)
        {
            GetPowerUpByType(powerUpType)?.Perform(tick);
        }

        private IPowerUpController GetPowerUpByType(PowerUpType powerUpType)
        {
            switch (powerUpType)
            {
                case PowerUpType.SonicSlap: return _sonicSlapPowerUpController;
                default: return default;
            }
        }
    }
}
