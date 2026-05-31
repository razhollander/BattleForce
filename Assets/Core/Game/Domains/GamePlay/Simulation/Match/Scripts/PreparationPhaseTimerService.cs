using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts
{
    public class PreparationPhaseTimerService : IPreparationPhaseTimerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public PreparationPhaseTimerService(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        public float PreparationPhaseTimer { get; set; }

        public void StepPreperationPhaseTimer(float deltaTime)
        {
            PreparationPhaseTimer -= deltaTime;
        }

        public bool IsTimerCompleted()
        {
            return PreparationPhaseTimer <= 0;
        }

        public void RestartTimer()
        {
            PreparationPhaseTimer = _gamePlayConfigService.GamePlayConfig.PreparationPhaseDuration;
        }
    }
}