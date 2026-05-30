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
    public class TrySendPlayerLockOnTargetChangedCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _gamePlayConfig;
        private INetEventsDataService _netEventsDataService;
        
        private FixedUnorderedList<ushort> _cachedLockedOnHeartIds;
        private readonly PhysicsBodyType[] _cachedBodyTypesRayCastCanHit = {PhysicsBodyType.PlayerHeart, PhysicsBodyType.Wall, PhysicsBodyType.PlayerSpaceship, PhysicsBodyType.StartMatchWall};
        private int _processedTick;
        private ushort _playerId;

        public TrySendPlayerLockOnTargetChangedCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }
        
        public TrySendPlayerLockOnTargetChangedCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            var networkConfig = _diContainer.Resolve<NetworkConfig>();
            _cachedLockedOnHeartIds = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentPlayers - 1);
        }

        public void Execute()
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_playerId);
            _cachedLockedOnHeartIds.Clear();
            GetTargetedEnemyIdsOfCaster(casterPlayerState, _cachedLockedOnHeartIds);
            _cachedLockedOnHeartIds.Sort();

            var casterTargetedEnemyIds = casterPlayerState.Spaceship.TargetedEnemyIds;
            var areIdentical = _cachedLockedOnHeartIds.IsIdentical(casterTargetedEnemyIds);

            if (areIdentical)
            {
                return;
            }

            casterTargetedEnemyIds.Clear();

            for (int i = 0; i < _cachedLockedOnHeartIds.Count; i++)
            {
                ref var targetedEnemyId = ref casterTargetedEnemyIds.AddAndGet();
                targetedEnemyId = _cachedLockedOnHeartIds[i];
            }
            
            _netEventsDataService.AddPlayerLockOnHeartTargetsChangedNetEvent(_processedTick, casterPlayerState.Id, casterTargetedEnemyIds);
        }

        private void GetTargetedEnemyIdsOfCaster(PlayerStateS2C casterPlayerState, FixedUnorderedList<ushort> outputTargetedEnemyIds)
        {
            var rayOriginPosition = casterPlayerState.Spaceship.Transform.GetHeadPosition();
            var radius = _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxRange;
            var maxLockOnHeartRangeSquare = radius * radius;

            DebugDrawUtils.DrawArc2D(rayOriginPosition, casterPlayerState.Spaceship.Transform.Direction, radius,
                _gamePlayConfig.PlayerSpaceship.LockOnHeartHalfArcAngleDegrees);

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
                var isEnemyHeartInRange = rayOriginToEnemyHeartDistanceSquared <= maxLockOnHeartRangeSquare;


                if (!isEnemyHeartInRange)
                {
                    continue;
                }

                var directionToEnemy = enemyHeartPos - rayOriginPosition;
                var playerCasterDirection = casterPlayerState.Spaceship.Transform.Direction;
                var deltaAngleRadians = MathUtils.DeltaAbsoluteAngleRadians(MathUtils.GetAngle(playerCasterDirection), MathUtils.GetAngle(directionToEnemy));
                var deltaAngleDegrees = deltaAngleRadians * Mathf.Rad2Deg;
                var maxLockOnHeartAngle = _gamePlayConfig.PlayerSpaceship.LockOnHeartHalfArcAngleDegrees;
                var isInAngleRange = deltaAngleDegrees <= maxLockOnHeartAngle;

                if (!isInAngleRange)
                {
                    continue;
                }

                var isTargetSpinned = targetedPlayerState.Spaceship.IsSpinned;
                if (!isTargetSpinned)
                {
                    var didRayTowardEnemyHeartHitAnything =_physicsSimulator.RayCast(rayOriginPosition, enemyHeartPos, out var hitBodyData, _cachedBodyTypesRayCastCanHit);
                    var didHitEnemyHeart = didRayTowardEnemyHeartHitAnything && hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart && hitBodyData.Id == targetedPlayerState.Id;
                    if (!didHitEnemyHeart)
                    {
                        continue;
                    }
                }
                
                ref var targetedPlayerId = ref outputTargetedEnemyIds.AddAndGet();
                targetedPlayerId = targetedPlayerState.Id;
            }
        }
    }
}