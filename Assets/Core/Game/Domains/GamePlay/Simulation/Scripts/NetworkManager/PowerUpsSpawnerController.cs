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
    public class PowerUpsSpawnerController : IPowerUpsSpawnerController
    {
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private float _timeSinceLastSpawn;

        public PowerUpsSpawnerController(
            SimulationGamePlayConfig gamePlayConfig)
        {
            _gamePlayConfig = gamePlayConfig;
        }

        public bool StepAndGetIsSpawnTimerEnded(float deltaTime)
        {
            _timeSinceLastSpawn += deltaTime;
            if (_timeSinceLastSpawn >= _gamePlayConfig.PowerUps.SpawnInterval)
            {
                _timeSinceLastSpawn = 0;
                return true;
            }

            return false;
        }
    }
}
