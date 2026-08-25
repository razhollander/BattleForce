using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class RotatePlayerTowardsMoveDestinationPointCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersMoveDestinationPointService _playersMoveDestinationPointService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private NetworkConfig _networkConfig;

        private ushort _playerId;

        public RotatePlayerTowardsMoveDestinationPointCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersMoveDestinationPointService = _diContainer.Resolve<IPlayersMoveDestinationPointService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            if (!_playersMoveDestinationPointService.TryGetPlayerMoveDestinationPoint(_playerId, out var destinationPointData))
            {
                return;
            }

            var playerSpaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            if (ShouldDropDestinationPoint(playerSpaceship, destinationPointData))
            {
                _playersMoveDestinationPointService.ClearPlayerMoveDestinationPoint(_playerId);
                return;
            }

            RotateTowardsDestinationPoint(playerSpaceship, destinationPointData.DestinationPoint);
        }

        private void RotateTowardsDestinationPoint(PlayerSpaceshipStateS2C playerSpaceship, Vector2 destinationPoint)
        {
            var currentDirection = playerSpaceship.Transform.Direction;
            var desiredDirection = (destinationPoint - playerSpaceship.Transform.Position).NormalizeSafe();
            var angleToDesiredDirectionInDegrees = MathUtils.GetSignedShortestAngleDegrees(currentDirection, desiredDirection);
            var maxRotationThisTickInDegrees = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var didReachDesiredRotation = MathF.Abs(angleToDesiredDirectionInDegrees) <= maxRotationThisTickInDegrees;

            if (didReachDesiredRotation)
            {
                playerSpaceship.Transform.Direction = desiredDirection;
                _playersMoveDestinationPointService.ClearPlayerMoveDestinationPoint(_playerId);
                return;
            }

            var rotationThisTickInDegrees = MathF.Sign(angleToDesiredDirectionInDegrees) * maxRotationThisTickInDegrees;
            var rotatedDirection = currentDirection.Rotate(rotationThisTickInDegrees);
            playerSpaceship.Transform.Direction = rotatedDirection;
            _playersMoveDestinationPointService.SetPlayerRotatedDirection(_playerId, rotatedDirection);
        }

        private bool ShouldDropDestinationPoint(PlayerSpaceshipStateS2C playerSpaceship, PlayerMoveDestinationPointData destinationPointData)
        {
            var wasPlayerTurnedByAnythingElse = playerSpaceship.Transform.Direction != destinationPointData.DirectionAfterLastRotation;
            var isPlayerStandingOnDestinationPoint = playerSpaceship.Transform.Position.IsAlmostEqual(destinationPointData.DestinationPoint);

            return !playerSpaceship.IsAlive
                   || playerSpaceship.TalentsState.IsSelectedTalentBlockingRotation()
                   || wasPlayerTurnedByAnythingElse
                   || isPlayerStandingOnDestinationPoint;
        }
    }
}
