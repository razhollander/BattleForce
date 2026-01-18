using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.UpdateService;
using Zenject;
using Random = UnityEngine.Random;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class PowerUpsSpawnerController : ITickable, IInitializable, IDisposable
    {
        private readonly PowerUpsConfig _config;
        private readonly SimulationStateS2C _simulationState;
        private readonly IMatchNetEventsDataService _matchNetEventsDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly IUpdateSubscriptionService _updateSubscriptionService;

        private float _timeSinceLastSpawn;
        private ushort _nextId = 0;

        public PowerUpsSpawnerController(
            PowerUpsConfig config,
            SimulationStateS2C simulationState,
            IMatchNetEventsDataService matchNetEventsDataService,
            IPhysicsSimulator physicsSimulator,
            IUpdateSubscriptionService updateSubscriptionService)
        {
            _config = config;
            _simulationState = simulationState;
            _matchNetEventsDataService = matchNetEventsDataService;
            _physicsSimulator = physicsSimulator;
            _updateSubscriptionService = updateSubscriptionService;
        }

        public void Initialize()
        {
            _updateSubscriptionService.RegisterTickable(this);
        }

        public void Dispose()
        {
            _updateSubscriptionService.UnregisterTickable(this);
        }

        public void Tick(float deltaTime)
        {
            HandleSpawning(deltaTime);
        }

        private void HandleSpawning(float deltaTime)
        {
            if (_simulationState.PowerUps.Count >= _config.MaxPowerUps) return;

            _timeSinceLastSpawn += deltaTime;
            if (_timeSinceLastSpawn >= _config.SpawnInterval)
            {
                _timeSinceLastSpawn = 0;
                SpawnPowerUp();
            }
        }

        private void SpawnPowerUp()
        {
            // Try to find a valid position
            Vector2 position = Vector2.Zero;
            bool validPositionFound = false;
            int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Range -15 to 15 (X) and -10 to 10 (Y) as assumed previously
                var candidate = new Vector2(Random.Range(-15f, 15f), Random.Range(-10f, 10f));
                if (_physicsSimulator.IsPositionFree(candidate, _config.Radius))
                {
                    position = candidate;
                    validPositionFound = true;
                    break;
                }
            }

            if (!validPositionFound) return; // Skip spawn if no valid position found

            Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            if (direction == Vector2.Zero) direction = new Vector2(1, 0);
            direction = Vector2.Normalize(direction);

            var id = _nextId++;
            var powerUp = new PowerUpS2C(id, position, _config.PowerUpType, direction, _config.Radius);

            _simulationState.PowerUps.Add(powerUp);
            _physicsSimulator.AddPowerUp(id, position, _config.Radius);

            // Set initial velocity
            var body = _physicsSimulator.GetPowerUp(id);
            if (body != null)
            {
                body.SetLinearVelocity(direction * _config.MoveSpeed);
            }

            _matchNetEventsDataService.AddEvent(new PowerUpSpawnedNetEventsS2C
            {
                Id = id,
                Type = _config.PowerUpType,
                Position = position
            });
        }
    }
}
