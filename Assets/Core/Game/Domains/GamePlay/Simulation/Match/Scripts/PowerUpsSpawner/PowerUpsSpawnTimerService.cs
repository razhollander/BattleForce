using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner
{
    public class PowerUpsSpawnTimerService : IPowerUpsSpawnerService
    {
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private float _secondsLeftUntilSpawn = 1;

        public PowerUpsSpawnTimerService(
            SimulationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
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
            var randomSeconds = Simulation.Scripts.RNG.RNG.NextFloat(_gamePlayConfig.PowerUps.SpawnMinSecondsInterval, _gamePlayConfig.PowerUps.SpawnMaxSecondsInterval);
            _secondsLeftUntilSpawn = randomSeconds;
        }
    }
}
