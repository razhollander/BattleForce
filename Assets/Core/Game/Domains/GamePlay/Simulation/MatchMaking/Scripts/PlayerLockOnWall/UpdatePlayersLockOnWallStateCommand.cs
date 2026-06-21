using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public class UpdatePlayersLockOnWallStateCommand : BaseCommand, ICommandVoid
    {
        private const int MAX_LOCK_ON_TARGETS = 1;

        private IMatchMakingDataService _matchMakingDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private ILockOnWallTimerService _lockOnWallTimerService;
        private INetEventsDataService _netEventsDataService;

        private FixedUnorderedList<ObjectLockedOnTargetS2C> _cachedWallTargets;
        private readonly PhysicsBodyType[] _cachedBodyTypesRayCastCanHit = {PhysicsBodyType.StartMatchWall, PhysicsBodyType.PlayerSpaceship};
        private int _processedTick;

        public UpdatePlayersLockOnWallStateCommand SetTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _lockOnWallTimerService = _diContainer.Resolve<ILockOnWallTimerService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _cachedWallTargets = new FixedUnorderedList<ObjectLockedOnTargetS2C>(MAX_LOCK_ON_TARGETS);
        }

        public void Execute()
        {
            var isWallEnabled = _matchMakingDataService.SimulationState.StartMatchWall.IsEnabled;

            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                _cachedWallTargets.Clear();

                var isLockingOnWall = isWallEnabled && IsPlayerLockingOnWall(playerState);
                if (isLockingOnWall)
                {
                    ref var target = ref _cachedWallTargets.AddAndGet();
                    target.PlayerTargetId = _sharedGamePlayConfig.MinEntityId;
                    target.IsLockOnTargetShootable = _lockOnWallTimerService.IsWallShootableByPlayer(playerState.Id);
                    target.TargetType = LockOnTargetType.StartMatchWall;
                }

                var targetedEnemyIds = playerState.Spaceship.ObjectsLockedOnTarget;
                if (_cachedWallTargets.IsIdentical(targetedEnemyIds))
                {
                    continue;
                }

                targetedEnemyIds.Clear();
                for (int i = 0; i < _cachedWallTargets.Count; i++)
                {
                    ref var targetedEnemy = ref targetedEnemyIds.AddAndGet();
                    targetedEnemy = _cachedWallTargets[i];
                }

                _netEventsDataService.AddPlayerLockOnHeartTargetsChangedNetEvent(_processedTick, playerState.Id, targetedEnemyIds);
            }
        }

        private bool IsPlayerLockingOnWall(MatchMakingPlayerStateS2C playerState)
        {
            var rayOriginPosition = playerState.Spaceship.Transform.GetHeadPosition();
            var wallCenter = System.Numerics.Vector2.Zero;

            var playerDirection = playerState.Spaceship.Transform.Direction;
            var directionToWall = wallCenter - rayOriginPosition;
            var deltaAngleRadians = MathUtils.DeltaAbsoluteAngleRadians(MathUtils.GetAngle(playerDirection), MathUtils.GetAngle(directionToWall));
            var deltaAngleDegrees = deltaAngleRadians * Mathf.Rad2Deg;
            var maxLockOnArcAngle = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnHeartHalfArcAngleDegrees;

            if (deltaAngleDegrees > maxLockOnArcAngle)
            {
                return false;
            }

            var didRayHitAnything = _physicsSimulator.RayCast(rayOriginPosition, wallCenter, out var hitBodyData, _cachedBodyTypesRayCastCanHit);
            var didHitWall = didRayHitAnything && hitBodyData.PhysicsBodyType == PhysicsBodyType.StartMatchWall && hitBodyData.Id == _sharedGamePlayConfig.MinEntityId;
            return didHitWall;
        }
    }
}
