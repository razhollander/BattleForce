using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.LockOnHeartTargetService
{
    public class LockOnHeartTargetService : ILockOnHeartTargetService
    {
        private readonly IMatchDataService _matchDataService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly FixedUnorderedList<ushort> _tempLockedOnHeartIds;

        public LockOnHeartTargetService(IMatchDataService matchDataService, IPhysicsSimulator physicsSimulator, SimulationGamePlayConfig gamePlayConfig, INetEventsDataService netEventsDataService)
        {
            _matchDataService = matchDataService;
            _physicsSimulator = physicsSimulator;
            _gamePlayConfig = gamePlayConfig;
            _netEventsDataService = netEventsDataService;
            // A temporary list to store locked-on targets before updating the state
            _tempLockedOnHeartIds = new FixedUnorderedList<ushort>(8);
        }

        public void Process(int processedTick, PlayerStateS2C playerState)
        {
            if (!playerState.Spaceship.IsAlive)
            {
                return;
            }

            _tempLockedOnHeartIds.Clear();

            var playerPos = playerState.Spaceship.Transform.Position;
            var playerDir = playerState.Spaceship.Transform.Direction;
            var rayStart = playerPos + playerDir * _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;

            var maxDistSq = _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxDistance * _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxDistance;
            var maxAngle = _gamePlayConfig.PlayerSpaceship.LockOnHeartMaxAngleDegrees;

            var players = _matchDataService.SimulationState.Players;
            for (int i = 0; i < players.Count; i++)
            {
                var enemyState = players[i];
                if (enemyState.Id == playerState.Id || !enemyState.Spaceship.IsAlive)
                {
                    continue;
                }

                var enemyHeartPos = enemyState.Spaceship.Transform.GetHeartPosition();
                var distSq = Vector2.DistanceSquared(rayStart, enemyHeartPos);

                if (distSq <= maxDistSq)
                {
                    var dirToEnemy = Vector2.Normalize(enemyHeartPos - rayStart);

                    // Vector2.Dot to calculate angle
                    var dot = Vector2.Dot(playerDir, dirToEnemy);
                    dot = Math.Clamp(dot, -1f, 1f);
                    var angleRad = Math.Acos(dot);
                    var angleDeg = angleRad * (180.0 / Math.PI);

                    if (angleDeg <= maxAngle)
                    {
                        var didHit = _physicsSimulator.RayCast(rayStart, enemyHeartPos, out var hitBodyData);
                        if (didHit && hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart && hitBodyData.Id == enemyState.Id)
                        {
                            _tempLockedOnHeartIds.Add(enemyState.Id);
                        }
                    }
                }
            }

            bool isChanged = false;
            var currentTargets = playerState.Spaceship.PlayerHeartsIdsOnTarget;

            if (_tempLockedOnHeartIds.Count != currentTargets.Count)
            {
                isChanged = true;
            }
            else
            {
                for (int i = 0; i < _tempLockedOnHeartIds.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < currentTargets.Count; j++)
                    {
                        if (_tempLockedOnHeartIds[i] == currentTargets[j])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        isChanged = true;
                        break;
                    }
                }
            }

            if (isChanged)
            {
                currentTargets.Clear();
                for (int i = 0; i < _tempLockedOnHeartIds.Count; i++)
                {
                    currentTargets.Add(_tempLockedOnHeartIds[i]);
                }
                _netEventsDataService.AddPlayerLockOnHeartTargetsChangedNetEvent(processedTick, playerState.Id, currentTargets);
            }
        }
    }
}
