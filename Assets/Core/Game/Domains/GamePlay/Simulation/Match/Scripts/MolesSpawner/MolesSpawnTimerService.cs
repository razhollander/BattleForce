using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public class MolesSpawnTimerService : IMolesSpawnTimerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private float _secondsLeftUntilSpawn;

        public MolesSpawnTimerService(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void StepTimer(float deltaTime)
        {
            _secondsLeftUntilSpawn -= deltaTime;
        }

        public bool IsSpawnTimerEnded()
        {
            return _secondsLeftUntilSpawn <= 0;
        }

        public void RestartSpawnTimer()
        {
            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            _secondsLeftUntilSpawn = RNG.NextFloat(whacAMoleConfig.MinMoleSpawnIntervalSeconds, whacAMoleConfig.MaxMoleSpawnIntervalSeconds);
        }
    }
}
