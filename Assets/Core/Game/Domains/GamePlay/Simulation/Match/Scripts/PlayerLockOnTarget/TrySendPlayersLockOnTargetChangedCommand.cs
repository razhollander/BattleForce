using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;


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
        private readonly PhysicsBodyType[] _cachedBodyTypesRayCastCanHit =
            {PhysicsBodyType.PlayerHeart, PhysicsBodyType.Wall, PhysicsBodyType.PlayerSpaceship, PhysicsBodyType.StartMatchWall, PhysicsBodyType.PowerUpBall, PhysicsBodyType.Mole};
        private IStageDataService _stageDataService;
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
            _stageDataService = _diContainer.Resolve<IStageDataService>();
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

                var isRock = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(playerState.Id, TalentType.Rock);
                var isFrozen = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(playerState.Id, TalentType.Frozen);
                var canPlayerFindTargets = !playerState.Spaceship.IsSpinned && playerState.Spaceship.IsAlive && !isRock && !isFrozen;
                if (canPlayerFindTargets)
                {
                    var rayOriginPosition = playerState.Spaceship.Transform.GetHeadPosition();
                    DebugDrawUtils.DrawArc2D(rayOriginPosition, playerState.Spaceship.Transform.Direction,
                        _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetMaxRange,
                        _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHalfArcAngleDegrees);
                    // Whac-A-Mole players cannot lock on each other, moles take the place of enemies as the shootable target.
                    if (_stageDataService.IsWhacAMoleStage)
                    {
                        FindTargetedMolesOfCaster(rayOriginPosition, playerState, _cachedLockedOnObjects);
                    }
                    else
                    {
                        FindTargetedEnemyIdsOfCaster(rayOriginPosition, playerState, _cachedLockedOnObjects);
                    }

                    FindTargetedPowerUpBallsOfCaster(rayOriginPosition, playerState, _cachedLockedOnObjects);
                }

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

        private void FindTargetedEnemyIdsOfCaster(Vector2 rayOriginPosition, PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedEnemyIds)
        {
            var casterBody = new PhysicsBodyData(casterPlayerState.Id, PhysicsBodyType.PlayerSpaceship);
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var targetedPlayerState = players[i];
                var isTargetRock = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(targetedPlayerState.Id, TalentType.Rock);
                var isTargetFrozen = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(targetedPlayerState.Id, TalentType.Frozen);
                var shouldTryTargetPlayer = targetedPlayerState.TeamId != casterPlayerState.TeamId && targetedPlayerState.Spaceship.IsAlive && !isTargetRock && !isTargetFrozen;

                if (!shouldTryTargetPlayer)
                {
                    continue;
                }

                var enemyHeartPos = targetedPlayerState.Spaceship.Transform.GetHeartPosition();
                if (!IsPositionInLockOnCone(casterPlayerState, rayOriginPosition, enemyHeartPos))
                {
                    continue;
                }

                var isTargetSpinned = targetedPlayerState.Spaceship.IsSpinned;
                var didRayTowardEnemyHeartHitAnything =_physicsSimulator.RayCast(rayOriginPosition, enemyHeartPos, out var hitBodyData, _cachedBodyTypesRayCastCanHit, casterBody);
                var didHitValidBody = isTargetSpinned
                    ? hitBodyData.PhysicsBodyType is PhysicsBodyType.PlayerHeart or PhysicsBodyType.PlayerSpaceship
                    : hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart;
                var didHitEnemyHeart = didRayTowardEnemyHeartHitAnything && didHitValidBody && hitBodyData.Id == targetedPlayerState.Id;
                if (!didHitEnemyHeart)
                {
                    continue;
                }

                AddLockedOnTarget(outputTargetedEnemyIds, casterPlayerState.Id, targetedPlayerState.Id, LockOnTargetType.Heart);
            }
        }

        private void FindTargetedPowerUpBallsOfCaster(Vector2 rayOriginPosition, PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedEnemyIds)
        {
            var casterBody = new PhysicsBodyData(casterPlayerState.Id, PhysicsBodyType.PlayerSpaceship);

            var powerUpBalls = _matchDataService.SimulationState.PowerUpBalls;

            for (int i = 0; i < powerUpBalls.Count; i++)
            {
                var powerUpBall = powerUpBalls.GetByIndex(i);
                var ballPosition = powerUpBall.Position;
                if (!IsPositionInLockOnCone(casterPlayerState, rayOriginPosition, ballPosition))
                {
                    continue;
                }

                var didRayTowardBallHitAnything = _physicsSimulator.RayCast(rayOriginPosition, ballPosition, out var hitBodyData, _cachedBodyTypesRayCastCanHit, casterBody);
                var didHitBall = didRayTowardBallHitAnything && hitBodyData.PhysicsBodyType == PhysicsBodyType.PowerUpBall && hitBodyData.Id == powerUpBall.Id;
                if (!didHitBall)
                {
                    continue;
                }

                AddLockedOnTarget(outputTargetedEnemyIds, casterPlayerState.Id, powerUpBall.Id, LockOnTargetType.PowerUpBall);
            }
        }

        private void FindTargetedMolesOfCaster(Vector2 rayOriginPosition, PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedObjects)
        {
            var casterBody = new PhysicsBodyData(casterPlayerState.Id, PhysicsBodyType.PlayerSpaceship);
            var moles = _matchDataService.SimulationState.Moles;

            for (int i = 0; i < moles.Count; i++)
            {
                var mole = moles.GetByIndex(i);
                var molePosition = mole.Position;

                if (!IsPositionInLockOnCone(casterPlayerState, rayOriginPosition, molePosition))
                {
                    continue;
                }

                var didRayTowardMoleHitAnything = _physicsSimulator.RayCast(rayOriginPosition, molePosition, out var hitBodyData, _cachedBodyTypesRayCastCanHit, casterBody);
                var didHitMole = didRayTowardMoleHitAnything && hitBodyData.PhysicsBodyType == PhysicsBodyType.Mole && hitBodyData.Id == mole.Id;

                if (!didHitMole)
                {
                    continue;
                }

                AddLockedOnTarget(outputTargetedObjects, casterPlayerState.Id, mole.Id, LockOnTargetType.Mole);
            }
        }

        private bool IsPositionInLockOnCone(PlayerStateS2C casterPlayerState, System.Numerics.Vector2 rayOriginPosition, System.Numerics.Vector2 targetPosition)
        {
            var maxRange = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetMaxRange;
            var rayOriginToTargetDistanceSquared = System.Numerics.Vector2.DistanceSquared(rayOriginPosition, targetPosition);
            var isTargetInRange = rayOriginToTargetDistanceSquared <= maxRange * maxRange;

            if (!isTargetInRange)
            {
                return false;
            }

            var directionToTarget = targetPosition - rayOriginPosition;
            var playerCasterDirection = casterPlayerState.Spaceship.Transform.Direction;
            var deltaAngleRadians = MathUtils.DeltaAbsoluteAngleRadians(MathUtils.GetAngle(playerCasterDirection), MathUtils.GetAngle(directionToTarget));
            var deltaAngleDegrees = deltaAngleRadians * UnityEngine.Mathf.Rad2Deg;
            var maxLockOnAngle = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHalfArcAngleDegrees;

            return deltaAngleDegrees <= maxLockOnAngle;
        }

        private void AddLockedOnTarget(FixedUnorderedList<ObjectLockedOnTargetS2C> outputTargetedEnemyIds, ushort casterId, ushort targetId, LockOnTargetType targetType)
        {
            ref var targetedObject = ref outputTargetedEnemyIds.AddAndGet();
            targetedObject.TargetId = targetId;
            targetedObject.IsLockOnTargetShootable = _lockOnTargetTimerService.IsTargetShootable(casterId, targetId, targetType);
            targetedObject.TargetType = targetType;
        }
    }
}