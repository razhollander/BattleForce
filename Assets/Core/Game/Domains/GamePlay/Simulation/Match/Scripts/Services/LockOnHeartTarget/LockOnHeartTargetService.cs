using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Utils;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.LockOnHeartTarget
{
    public class LockOnHeartTargetService : ILockOnHeartTargetService
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly FixedUnorderedList<ushort> _cachedLockedOnHeartIds;
        private static readonly PhysicsBodyType[] _bodyTypesRayCastCanHit = {PhysicsBodyType.PlayerHeart, PhysicsBodyType.Wall, PhysicsBodyType.StartMatchWall};
        
        public LockOnHeartTargetService(IMatchDataService matchDataService, IPhysicsSimulator physicsSimulator, SimulationGamePlayConfig gamePlayConfig, INetEventsDataService netEventsDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _physicsSimulator = physicsSimulator;
            _gamePlayConfig = gamePlayConfig;
            _netEventsDataService = netEventsDataService;
            _cachedLockedOnHeartIds = new FixedUnorderedList<ushort>(networkConfig.MaxCap.ConcurrentPlayers - 1);
        }

        public void Process(int processedTick, PlayerStateS2C casterPlayerState)
        {
            if (!casterPlayerState.Spaceship.IsAlive)
            {
                return;
            }

            _cachedLockedOnHeartIds.Clear();
            GetTargetedEnemyIds(casterPlayerState, _cachedLockedOnHeartIds);
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

            _netEventsDataService.AddPlayerLockOnHeartTargetsChangedNetEvent(processedTick, casterPlayerState.Id, casterTargetedEnemyIds);
        }

        private void GetTargetedEnemyIds(PlayerStateS2C casterPlayerState, FixedUnorderedList<ushort> outputTargetedEnemyIds)
        {
            var rayDirection = casterPlayerState.Spaceship.Transform.Direction;
            var rayOriginPosition = casterPlayerState.Spaceship.Transform.GetHeadPosition();

            var maxLockOnHeartRangeSquare = _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxRange * _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxRange;

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
                var rayOriginToEnemyHeartDistanceSquared = Vector2.DistanceSquared(rayOriginPosition, enemyHeartPos);
                var isEnemyHeartInRange = rayOriginToEnemyHeartDistanceSquared <= maxLockOnHeartRangeSquare;

                if (!isEnemyHeartInRange)
                {
                    continue;
                }

                var directionToEnemy = enemyHeartPos - rayOriginPosition;
                var deltaAngleRadians = MathUtils.DeltaAngleRadians(MathUtils.GetAngle(rayDirection), MathUtils.GetAngle(directionToEnemy));
                var deltaAngleDegrees = deltaAngleRadians * Mathf.Rad2Deg;
                // var directionToEnemy = Vector2.Normalize(enemyHeartPos - rayOriginPosition);
                // var dot = Vector2.Dot(rayDirection, directionToEnemy);
                // dot = Math.Clamp(dot, -1f, 1f);
                // var angleRad = Math.Acos(dot);
                // var angleDeg = angleRad * (180.0 / Math.PI);
                var maxLockOnHeartAngle = _gamePlayConfig.PlayerSpaceship.LockOnHeartHalfArcAngleDegrees;
                var isInAngleRange = deltaAngleDegrees <= maxLockOnHeartAngle;

                if (!isInAngleRange)
                {
                    continue;
                }

                var didHit = _physicsSimulator.RayCast(rayOriginPosition, enemyHeartPos, out var hitBodyData, _bodyTypesRayCastCanHit);
                var didHitEnemyHeart = didHit && hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart && hitBodyData.Id == targetedPlayerState.Id;

                if (!didHitEnemyHeart)
                {
                    continue;
                }

                ref var targetedPlayerId = ref outputTargetedEnemyIds.AddAndGet();
                targetedPlayerId = targetedPlayerState.Id;
            }
        }
    }
}
