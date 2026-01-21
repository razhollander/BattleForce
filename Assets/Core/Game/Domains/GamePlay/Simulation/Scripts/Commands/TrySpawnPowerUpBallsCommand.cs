using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class TrySpawnPowerUpBallsCommand : BaseCommand, ICommandVoid
    {
        private const int MAX_ATTEMPTS_TO_FIND_FREE_SPAWN_POSITION = 1000;

        private SimulationGamePlayConfig _gamePlayConfig;
        private IPowerUpsSpawnerService _iPowerUpsSpawnerService;

        private int _processedTick;
        private IPhysicsSimulator _physicsSimulator;
        private IMatchDataService _matchDataService;
        private IMatchNetEventsDataService _matchNetEventsDataService;

        public TrySpawnPowerUpBallsCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _iPowerUpsSpawnerService = _diContainer.Resolve<IPowerUpsSpawnerService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            var isSpawnTimerEnded = _iPowerUpsSpawnerService.IsSpawnTimerEnded();
            if (isSpawnTimerEnded)
            {
                _iPowerUpsSpawnerService.RestartSpawnTimer();
            }
            
            var areCurrentlyMaxPowerUpBalls = _matchDataService.SimulationState.PowerUpBalls.Count >= _gamePlayConfig.PowerUps.MaxConcurrentPowerUpBalls;
            var shouldSpawnPowerUpBall = isSpawnTimerEnded && !areCurrentlyMaxPowerUpBalls;

            if (shouldSpawnPowerUpBall)
            {
                SpawnPowerUp();
            }
        }

        private void SpawnPowerUp()
        {
            if (!TryGenerateRandomPowerUpBall(out var position, out var velocity, out var powerUpType))
            {
                return;
            }

            var powerUpBall = _matchDataService.AddPowerUpBall(position, velocity, powerUpType);
            _physicsSimulator.AddPowerUpBall(powerUpBall.Id, position, velocity, _gamePlayConfig.PowerUps.Radius);
            _matchNetEventsDataService.AddPowerUpSpawnedNetEvent(_processedTick, powerUpBall.Id, position);
        }

        private bool TryGenerateRandomPowerUpBall(out Vector2 position, out Vector2 velocity, out PowerUpType powerUpType)
        {
            position = default;
            velocity = default;
            powerUpType = default;

            if (!TryFindAvailablePosition(out position))
            {
                return false;
            }

            var directionAngle = RNG.NextFloat(0f, 360f);
            var direction = directionAngle.FromAngleRadians();
            velocity = direction * _gamePlayConfig.PowerUps.MoveSpeed;
            var values = (PowerUpType[]) Enum.GetValues(typeof(PowerUpType));
            powerUpType = values[RNG.NextInt(values.Length)];

            return true;
        }

        private bool TryFindAvailablePosition(out Vector2 position)
        {
            position = Vector2.Zero;
            var maxAttempts = MAX_ATTEMPTS_TO_FIND_FREE_SPAWN_POSITION;
            var environmentHalfSize = _matchDataService.Environment.EnvironmentHalfSize;
            var powerUpsRadius = _gamePlayConfig.PowerUps.Radius;

            for (var i = 0; i < maxAttempts; i++)
            {
                var randomCandidatePosition = new Vector2(RNG.NextFloat(-environmentHalfSize.X, environmentHalfSize.X),
                    RNG.NextFloat(-environmentHalfSize.Y, environmentHalfSize.Y));

                if (!_physicsSimulator.IsSquareHitAnyBodyTypes(randomCandidatePosition, powerUpsRadius, PhysicsBodyType.Wall, PhysicsBodyType.PlayerBullet))
                {
                    position = randomCandidatePosition;

                    return true;
                }
            }

            return false;
        }
    }
}