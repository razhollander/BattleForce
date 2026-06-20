using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall
{
    public class UpdatePlayersLockOnWallStateCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IPhysicsSimulator _physicsSimulator;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private ILockOnWallTimerService _lockOnWallTimerService;

        private readonly PhysicsBodyType[] _cachedBodyTypesRayCastCanHit = {PhysicsBodyType.StartMatchWall, PhysicsBodyType.PlayerSpaceship};

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _lockOnWallTimerService = _diContainer.Resolve<ILockOnWallTimerService>();
        }

        public void Execute()
        {
            var isWallEnabled = _matchMakingDataService.SimulationState.StartMatchWall.IsEnabled;

            foreach (var playerState in _matchMakingDataService.SimulationState.Players.AsSpan())
            {
                var isLockingOnWall = isWallEnabled && IsPlayerLockingOnWall(playerState);
                playerState.Spaceship.IsLockingOnWall = isLockingOnWall;
                playerState.Spaceship.IsLockingOnWallShootable = isLockingOnWall && _lockOnWallTimerService.IsShootable(playerState.Id);
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
