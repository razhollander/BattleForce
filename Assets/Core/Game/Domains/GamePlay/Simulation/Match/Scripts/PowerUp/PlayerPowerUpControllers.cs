using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp
{
    public class PlayerPowerUpControllers
    {
        private readonly SonicSlapPowerUpController _sonicSlapPowerUpController;
        private readonly GalacticPullPowerUpController _galacticPullPowerUpController;
        private readonly NukePowerUpController _nukePowerUpController;

        public PlayerPowerUpControllers(IMatchDataService matchDataService, INetEventsDataService netEventsDataService, NetworkConfig networkConfig, ISimulationGamePlayConfigService gamePlayConfigService, ICommandFactory commandFactory)
        {
            _sonicSlapPowerUpController = new SonicSlapPowerUpController(matchDataService, netEventsDataService, networkConfig);
            _galacticPullPowerUpController = new GalacticPullPowerUpController(matchDataService, netEventsDataService, gamePlayConfigService, networkConfig);
            _nukePowerUpController = new NukePowerUpController(matchDataService, netEventsDataService, gamePlayConfigService, commandFactory);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _sonicSlapPowerUpController.SetCasterId(casterPlayerId);
            _galacticPullPowerUpController.SetCasterId(casterPlayerId);
            _nukePowerUpController.SetCasterId(casterPlayerId);
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
                case PowerUpType.GalacticPull: return _galacticPullPowerUpController;
                case PowerUpType.Nuke: return _nukePowerUpController;
                default: return default;
            }
        }
    }
}
