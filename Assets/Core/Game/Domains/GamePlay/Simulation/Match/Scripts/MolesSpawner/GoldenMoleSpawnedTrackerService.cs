using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    /// <summary>
    /// Decides which mole comes out golden. A golden mole appears once a random amount of regular moles has spawned
    /// since the last one, and a fresh amount is rolled every time one does.
    /// </summary>
    public class GoldenMoleSpawnedTrackerService : IGoldenMoleSpawnedTrackerService
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private int _regularMolesUntilGolden;
        private int _regularMolesSpawnedSinceGolden;

        public GoldenMoleSpawnedTrackerService(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        // The first target is rolled here rather than on the first spawn, so the sim RNG is already seeded by the time
        // it is read and ShouldSpawnGoldenMole stays a pure query.
        public void ResetGoldenMoleSpawnCounter()
        {
            _regularMolesSpawnedSinceGolden = 0;
            RollGoldenTarget();
        }

        public bool ShouldSpawnGoldenMole()
        {
            return _regularMolesSpawnedSinceGolden >= _regularMolesUntilGolden;
        }

        public void RegisterMoleSpawned(bool isGolden)
        {
            if (isGolden)
            {
                ResetGoldenMoleSpawnCounter();
                return;
            }

            _regularMolesSpawnedSinceGolden++;
        }

        private void RollGoldenTarget()
        {
            var whacAMoleConfig = _gamePlayConfigService.GamePlayConfig.WhacAMole;
            _regularMolesUntilGolden = RNG.NextInt(whacAMoleConfig.MinMolesUntilGoldenMole, whacAMoleConfig.MaxMolesUntilGoldenMole + 1);
        }
    }
}
