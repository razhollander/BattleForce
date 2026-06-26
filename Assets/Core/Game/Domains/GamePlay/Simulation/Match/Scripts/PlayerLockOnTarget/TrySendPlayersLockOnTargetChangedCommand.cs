using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class TrySendPlayersLockOnTargetChangedCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private INetEventsDataService _netEventsDataService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;

        private FixedUnorderedList<ObjectLockedOnTargetS2C> _cachedLockedOnObjects;
        private readonly PhysicsBodyType[] _cachedBodyTypesRayCastCanHit = {PhysicsBodyType.PlayerHeart, PhysicsBodyType.Wall, PhysicsBodyType.PlayerSpaceship, PhysicsBodyType.StartMatchWall, PhysicsBodyType.PowerUpBall};
        private int _processedTick;

        public TrySendPlayersLockOnTargetChangedCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            var networkConfig = _diContainer.Resolve<NetworkConfig>();
            _cachedLockedOnObjects = new FixedUnorderedList<ObjectLockedOnTargetS2C>(networkConfig.MaxCap.ConcurrentLockOnTargets);
        }

        public void Execute()
        {
            if (_matchDataService.SimulationState.IsInPreparationPhase)
            {
                return;
            }
            
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                _cachedLockedOnObjects.Clear();
                FindTargetedEnemyIdsOfCaster(playerState, _cachedLockedOnObjects);
                FindTargetedPowerUpBallsOfCaster(playerState, _cachedLockedOnObjects);
                _cachedLockedOnObjects.Sort();

                var casterTargetedObjects = playerState.Spaceship.LockOnTargetObjects;
                var areIdentical = _cachedLockedOnObjects.IsIdentical(casterTargetedObjects);

                if (areIdentical)
                {
                    continue;
                }

                casterTargetedObjects.Clear();

                for (int i = 0; i < _cachedLockedOnObjects.Count; i++)
                {
                    ref var targetedEnemy = ref casterTargetedObjects.AddAndGet();
                    targetedEnemy = _cachedLockedOnObjects[i];
                }

                _netEventsDataService.AddPlayerLockOnTargetsChangedNetEvent(_processedTick, playerState.Id, casterTargetedObjects);
            }
        }

        private void FindTargetedEnemyIdsOfCaster(PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedEnemyIds)
        {
            if (casterPlayerState.Spaceship.IsSpinned || !casterPlayerState.Spaceship.IsAlive)
            {
                return;
            }
            
            var rayOriginPosition = casterPlayerState.Spaceship.Transform.GetHeadPosition();
            var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetMaxRange;
            var maxLockOnTargetRangeSquare = radius * radius;
            
            DebugDrawUtils.DrawArc2D(rayOriginPosition, casterPlayerState.Spaceship.Transform.Direction, radius,
                _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHalfArcAngleDegrees);

            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var targetedPlayerState = players[i];
                var shouldTryTargetPlayer = targetedPlayerState.TeamId != casterPlayerState.TeamId && targetedPlayerState.Spaceship.IsAlive;

                if (!shouldTryTargetPlayer)
                {
                    continue;
                }

                var enemyHeartPos = targetedPlayerState.Spaceship.Transform.GetHeartPosition();
                var rayOriginToEnemyHeartDistanceSquared = System.Numerics.Vector2.DistanceSquared(rayOriginPosition, enemyHeartPos);
                var isEnemyHeartInRange = rayOriginToEnemyHeartDistanceSquared <= maxLockOnTargetRangeSquare;
                
                if (!isEnemyHeartInRange)
                {
                    continue;
                }

                var directionToEnemy = enemyHeartPos - rayOriginPosition;
                var playerCasterDirection = casterPlayerState.Spaceship.Transform.Direction;
                var deltaAngleRadians = MathUtils.DeltaAbsoluteAngleRadians(MathUtils.GetAngle(playerCasterDirection), MathUtils.GetAngle(directionToEnemy));
                var deltaAngleDegrees = deltaAngleRadians * Mathf.Rad2Deg;
                var maxLockOnTargetAngle = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHalfArcAngleDegrees;
                var isInAngleRange = deltaAngleDegrees <= maxLockOnTargetAngle;

                if (!isInAngleRange)
                {
                    continue;
                }

                var isTargetSpinned = targetedPlayerState.Spaceship.IsSpinned;
                var didRayTowardEnemyHeartHitAnything =_physicsSimulator.RayCast(rayOriginPosition, enemyHeartPos, out var hitBodyData, _cachedBodyTypesRayCastCanHit);
                var didHitValidBody = isTargetSpinned 
                    ? hitBodyData.PhysicsBodyType is PhysicsBodyType.PlayerHeart or PhysicsBodyType.PlayerSpaceship 
                    : hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart;
                var didHitEnemyHeart = didRayTowardEnemyHeartHitAnything && didHitValidBody && hitBodyData.Id == targetedPlayerState.Id;
                if (!didHitEnemyHeart)
                {
                    continue;
                }
                
                ref var targetedPlayer = ref outputTargetedEnemyIds.AddAndGet();
                targetedPlayer.TargetId = targetedPlayerState.Id;
                targetedPlayer.IsLockOnTargetShootable = _lockOnTargetTimerService.IsTargetShootable(casterPlayerState.Id, targetedPlayerState.Id, LockOnTargetType.Heart);
                targetedPlayer.TargetType = LockOnTargetType.Heart;
            }
        }

        private void FindTargetedPowerUpBallsOfCaster(PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedEnemyIds)
        {
            if (casterPlayerState.Spaceship.IsSpinned || !casterPlayerState.Spaceship.IsAlive)
            {
                return;
            }

            var rayOriginPosition = casterPlayerState.Spaceship.Transform.GetHeadPosition();
            var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetMaxRange;
            var maxLockOnRangeSquare = radius * radius;
            var maxLockOnAngle = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHalfArcAngleDegrees;
            var playerCasterDirection = casterPlayerState.Spaceship.Transform.Direction;

            var powerUpBalls = _matchDataService.SimulationState.PowerUpBalls;

            for (int i = 0; i < powerUpBalls.Count; i++)
            {
                var powerUpBall = powerUpBalls.GetByIndex(i);
                var ballPosition = powerUpBall.Position;
                var rayOriginToBallDistanceSquared = System.Numerics.Vector2.DistanceSquared(rayOriginPosition, ballPosition);
                var isBallInRange = rayOriginToBallDistanceSquared <= maxLockOnRangeSquare;

                if (!isBallInRange)
                {
                    continue;
                }

                var directionToBall = ballPosition - rayOriginPosition;
                var deltaAngleRadians = MathUtils.DeltaAbsoluteAngleRadians(MathUtils.GetAngle(playerCasterDirection), MathUtils.GetAngle(directionToBall));
                var deltaAngleDegrees = deltaAngleRadians * Mathf.Rad2Deg;
                var isInAngleRange = deltaAngleDegrees <= maxLockOnAngle;

                if (!isInAngleRange)
                {
                    continue;
                }

                var didRayTowardBallHitAnything = _physicsSimulator.RayCast(rayOriginPosition, ballPosition, out var hitBodyData, _cachedBodyTypesRayCastCanHit);
                var didHitBall = didRayTowardBallHitAnything && hitBodyData.PhysicsBodyType == PhysicsBodyType.PowerUpBall && hitBodyData.Id == powerUpBall.Id;
                if (!didHitBall)
                {
                    continue;
                }

                ref var targetedBall = ref outputTargetedEnemyIds.AddAndGet();
                targetedBall.TargetId = powerUpBall.Id;
                targetedBall.IsLockOnTargetShootable = _lockOnTargetTimerService.IsTargetShootable(casterPlayerState.Id, powerUpBall.Id, LockOnTargetType.PowerUpBall);
                targetedBall.TargetType = LockOnTargetType.PowerUpBall;
            }
        }
    }
}