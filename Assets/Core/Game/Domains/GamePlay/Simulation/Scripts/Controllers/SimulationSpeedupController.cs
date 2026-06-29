using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationSpeedMultiplier;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers
{
    // Applies the configured simulation speedup multiplier, but only once a client has connected.
    // Until then the simulation runs at normal (1x) speed.
    public class SimulationSpeedupController : ISimulationSpeedupController
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly ISimulationSpeedMultiplierDataService _speedMultiplierDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public SimulationSpeedupController(IServerNetworkManager networkManager,
            ISimulationSpeedMultiplierDataService speedMultiplierDataService,
            ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _networkManager = networkManager;
            _speedMultiplierDataService = speedMultiplierDataService;
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void InitEntryPoint()
        {
            _networkManager.OnClientPeerConnectedEvent += OnClientPeerConnected;
        }

        public void InitExitPoint()
        {
            _networkManager.OnClientPeerConnectedEvent -= OnClientPeerConnected;
        }

        private void OnClientPeerConnected()
        {
            var speedup = _gamePlayConfigService.GamePlayConfig.SpeedupSimulation;
            _speedMultiplierDataService.SetMultiplier(speedup);
            LogService.LogTopic($"[SimulationSpeedupController] Client connected, applying simulation speedup x{speedup}", LogTopicType.ServerNetwork);
        }
    }
}
