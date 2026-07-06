using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers
{
    // Keeps the tick service in sync with the configured simulation speedup.
    // The initial speedup is applied when the tick starts; in the editor this also reacts to live tweaks
    // of SpeedupSimulation, forwarded through ISimulationGamePlayConfigService.
    public class SimulationSpeedupController : ISimulationSpeedupController
    {
        private readonly ITickService _tickService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public SimulationSpeedupController(ITickService tickService,
            ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _tickService = tickService;
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void InitEntryPoint()
        {
            _gamePlayConfigService.OnSpeedupSimulationChangedEvent += OnSpeedupSimulationChanged;
        }

        public void InitExitPoint()
        {
            _gamePlayConfigService.OnSpeedupSimulationChangedEvent -= OnSpeedupSimulationChanged;
        }

        private void OnSpeedupSimulationChanged(float speedup)
        {
            _tickService.SetSpeedMultiplier(speedup);
            LogService.LogTopic($"[SimulationSpeedupController] Simulation speedup changed to x{speedup}", LogTopicType.ServerNetwork);
        }
    }
}
