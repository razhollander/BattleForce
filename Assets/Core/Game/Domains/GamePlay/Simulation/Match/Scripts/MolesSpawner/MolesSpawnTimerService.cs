using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public class MolesSpawnTimerService : IMolesSpawnerService
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
            _secondsLeftUntilSpawn = _gamePlayConfigService.GamePlayConfig.WhacAMole.MoleSpawnIntervalSeconds;
        }
    }
}
