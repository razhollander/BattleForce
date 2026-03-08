using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts
{
    public class PreparationPhaseTimerService : IPreparationPhaseTimerService
    {
        private readonly SimulationGamePlayConfig _gamePlayConfig;

        public PreparationPhaseTimerService(SimulationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
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
            PreparationPhaseTimer = _gamePlayConfig.PreparationPhaseDuration;
        }
    }
}