using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner
{
    public class PowerUpsSpawnTimerService : IPowerUpsSpawnerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private float _secondsLeftUntilSpawn = 1;

        public PowerUpsSpawnTimerService(
            ISimulationGamePlayConfigService gamePlayConfigService)
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
            var randomSeconds = Simulation.Scripts.RNG.RNG.NextFloat(_gamePlayConfigService.GamePlayConfig.PowerUps.SpawnMinSecondsInterval, _gamePlayConfigService.GamePlayConfig.PowerUps.SpawnMaxSecondsInterval);
            _secondsLeftUntilSpawn = randomSeconds;
        }
    }
}
