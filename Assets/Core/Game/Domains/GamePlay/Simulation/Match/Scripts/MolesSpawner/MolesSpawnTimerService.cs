using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public class MolesSpawnTimerService : IMolesSpawnerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private float _secondsLeftUntilSpawn;
        private int _regularMolesUntilGolden = -1; // -1 until the first target is rolled, so the sim RNG is already seeded
        private int _regularMolesSpawnedSinceGolden;

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

        public bool ShouldSpawnGoldenMole()
        {
            EnsureGoldenTargetRolled();
            return _regularMolesSpawnedSinceGolden >= _regularMolesUntilGolden;
        }

        public void RegisterMoleSpawned(bool isGolden)
        {
            if (isGolden)
            {
                _regularMolesSpawnedSinceGolden = 0;
                RollGoldenTarget();
                return;
            }

            _regularMolesSpawnedSinceGolden++;
        }

        private void EnsureGoldenTargetRolled()
        {
            if (_regularMolesUntilGolden < 0)
            {
                RollGoldenTarget();
            }
        }

        private void RollGoldenTarget()
        {
            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            _regularMolesUntilGolden = RNG.NextInt(whacAMoleConfig.MinMolesUntilGoldenMole, whacAMoleConfig.MaxMolesUntilGoldenMole + 1);
        }
    }
}
