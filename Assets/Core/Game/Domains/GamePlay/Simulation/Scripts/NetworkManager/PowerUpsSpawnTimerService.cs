using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.UpdateService;
using Zenject;
using Random = UnityEngine.Random;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
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
            var randomSeconds = RNG.NextFloat(_gamePlayConfig.PowerUps.SpawnMinSecondsInterval, _gamePlayConfig.PowerUps.SpawnMaxSecondsInterval);
            _secondsLeftUntilSpawn = randomSeconds;
        }
    }
}
